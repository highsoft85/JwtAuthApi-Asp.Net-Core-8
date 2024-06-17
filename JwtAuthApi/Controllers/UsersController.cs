using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using JwtAuthApi.Services;
using JwtAuthApi.Models;
using JwtAuthApi.Data;
using Asp.Versioning;
using Azure.Core;
using System.Security.Claims;

namespace JwtAuthApi.Controllers;

[ApiVersion(1.0)]
[Route("api/[controller]")]
[ApiController]
public class UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ITokenService tokenService, ILogger<UsersController> logger) 
    : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ILogger<UsersController> _logger = logger;

    [HttpGet("All")]
    [Authorize(Roles = "Admin")]
    public async Task<List<ApplicationUser>> Index()
    {
        var users = await _userManager.Users.ToListAsync();

        return users;
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userManager.CreateAsync(
            new ApplicationUser { UserName = request.Username, Email = request.Email, Role = request.Role, Hobby = request.Hobby },
            request.Password!
        );

        if (result.Succeeded)
        {
            request.Password = "";
            return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await _userManager.FindByEmailAsync(request.Email!);
        if (managedUser == null)
        {
            return BadRequest("Bad credentials");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password!);
        if (!isPasswordValid)
        {
            return BadRequest("Bad credentials");
        }

        var userInDb = _dbContext.Users.FirstOrDefault(u => u.Email == request.Email);

        if (userInDb is null)
        {
            return Unauthorized();
        }

        var accessToken = _tokenService.CreateToken(userInDb);
        var refreshToken = _tokenService.CreateRefreshToken();

        managedUser.RefreshToken = refreshToken;
        managedUser.RefreshTokenExpiryTime = DateTime.Now.AddDays(Convert.ToInt16(_tokenService.GetConfigurationValue("JwtTokenSettings:RefreshExpirationDays")));
        await _userManager.UpdateAsync(managedUser);

        return Ok(new AuthResponse
        {
            Email = userInDb.Email,
            Token = accessToken,
            RefreshToken = refreshToken
        });
    }
    
    [HttpDelete("{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Ok();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost, AllowAnonymous]
    [Route("Refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] TokenRefreshRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string accessToken = request.AccessToken!;
        string refreshToken = request.RefreshToken!;

        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);

        //var userName = principal.Identity?.Name;   // this is mapped to the Name claim by default
        var userId = principal.FindFirstValue("user_id");

        var managedUser = await _userManager.FindByIdAsync(userId!);
        if (managedUser is null || managedUser.RefreshToken != refreshToken || managedUser.RefreshTokenExpiryTime <= DateTime.Now)
            return BadRequest("Invalid client request");

        accessToken = _tokenService.CreateToken(managedUser);
        refreshToken = _tokenService.CreateRefreshToken();
        managedUser.RefreshToken = refreshToken;
        managedUser.RefreshTokenExpiryTime = DateTime.Now.AddDays(Convert.ToInt16(_tokenService.GetConfigurationValue("JwtTokenSettings:RefreshExpirationDays")));
        await _userManager.UpdateAsync(managedUser);

        return Ok(new AuthResponse
        {
            Email = managedUser.Email,
            Token = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost, Authorize]
    [Route("Revoke")]
    public async Task<IActionResult> Revoke()
    {
        //var username = ApplicationUser.Identity.id;
        var userId = "";
        var managedUser = await _userManager.FindByIdAsync(userId);
        if (managedUser == null) return BadRequest();
        managedUser.RefreshToken = null;
        await _userManager.UpdateAsync(managedUser);

        return NoContent();
    }
}
