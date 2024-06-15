using JwtAuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using JwtAuthApi.Models;

namespace JwtAuthApi.Controllers;

//[ApiVersion(1.0)]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService) 
    {
        _userService = userService;
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public IActionResult Login(User user)
    {
        var token = _userService.Login(user);
        if (token == null || token == string.Empty)
        {
            return BadRequest(new { message = "Username or Password is incorrect" });
        }
        return Ok(token);
    }
}
