using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using JwtAuthApi.Models;

namespace JwtAuthApi.Services;

public class UserService(IConfiguration configuration, ILogger<UserService> logger) : IUserService
{
    private readonly ILogger<UserService> _logger = logger;

    private readonly IConfiguration _configuration = configuration;

    public string CreateToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var expiration = DateTime.UtcNow.AddMinutes(Int16.Parse(_configuration["JwtTokenSettings:ExpirationMinutes"]!));
        var jwtSub = _configuration["JwtTokenSettings:JwtRegisteredClaimNamesSub"];
        var key = Encoding.UTF8.GetBytes(_configuration["JwtTokenSettings:SymmetricSecurityKey"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration.GetSection("JwtTokenSettings")["ValidIssuer"],
            Audience = _configuration.GetSection("JwtTokenSettings")["ValidAudience"],
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwtSub!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("user_id", user.Id),
                new Claim("hobby", user.Hobby ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        string userToken = tokenHandler.WriteToken(token);

        _logger.LogInformation("JWT Token created");
        Console.Out.WriteLine("=====================");
        Console.Out.WriteLine(userToken);

        return userToken;
    }
}
