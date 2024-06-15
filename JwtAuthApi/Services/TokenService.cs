using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using JwtAuthApi.Models;

namespace JwtAuthApi.Services;

public class TokenService(IConfiguration configuration, ILogger<TokenService> logger) : ITokenService
{
    private readonly ILogger<TokenService> _logger = logger;

    private readonly IConfiguration _configuration = configuration;

    public string CreateToken(ApplicationUser user)
    {
        var expiration = DateTime.UtcNow.AddMinutes(Int16.Parse(_configuration["JwtTokenSettings:ExpirationMinutes"]!));
        var jwtSub = _configuration["JwtTokenSettings:JwtRegisteredClaimNamesSub"];
        var key = Encoding.UTF8.GetBytes(_configuration["JwtTokenSettings:SymmetricSecurityKey"]!);
        var token = new JwtSecurityToken(
            issuer: _configuration.GetSection("JwtTokenSettings")["ValidIssuer"],
            audience: _configuration.GetSection("JwtTokenSettings")["ValidAudience"],
            expires: expiration,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            claims: [
                new Claim(JwtRegisteredClaimNames.Sub, jwtSub!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("user_id", user.Id),
                new Claim("hobby", user.Hobby ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogInformation("JWT Token created");
        Console.Out.WriteLine("=====================");
        Console.Out.WriteLine(tokenString);

        return tokenString;
    }
}
