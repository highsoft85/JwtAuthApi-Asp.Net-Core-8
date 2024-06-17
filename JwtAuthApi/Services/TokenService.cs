using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using JwtAuthApi.Models;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

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
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }

    public string CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,          // If you want to allow a certain amount of clock drift, set that here
            ValidateIssuer = true,              // Validate the JWT Issuer (iss) claim
            ValidateAudience = true,            // Validate the JWT Audience (aud) claim
            ValidateLifetime = false,           // Validate the token expiry
            ValidateIssuerSigningKey = true,    // The signing key must match!
            ValidIssuer = configuration["JwtTokenSettings:ValidIssuer"],
            ValidAudience = configuration["JwtTokenSettings:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtTokenSettings:SymmetricSecurityKey"]!))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        // JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

//        if (jwtSecurityToken == null || jwtSecurityToken?.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
//            throw new SecurityTokenException("Invalid token");

        if (securityToken == null)
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public Object? GetConfigurationValue(string key)
    {
        return configuration[key];
    }
}
