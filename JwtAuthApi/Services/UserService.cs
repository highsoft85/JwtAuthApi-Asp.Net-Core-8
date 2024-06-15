using JwtAuthApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthApi.Services;

public class UserService : IUserService
{
    private const int ExpirationMinutes = 30;
    private readonly ILogger<UserService> _logger;

    private List<User> _users = new List<User>
    {
        new User
        {
            UserName = "admin",
            Email = "admin@test.com",
            Password = "administrator"
        },
        new User
        {
            UserName = "user",
            Email = "user@test.com",
            Password = "user"
        }
    };

    private readonly IConfiguration _configuration;

    public UserService(IConfiguration configuration, ILogger<UserService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string Login(User user)
    {
        var LoginUser = _users.SingleOrDefault(x => x.UserName == user.UserName && x.Password == user.Password);

        if (LoginUser == null)
        {
            return string.Empty;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var expiration = DateTime.UtcNow.AddMinutes(Int16.Parse(_configuration["JwtTokenSettings:ExpirationMinutes"]));
        var jwtSub = _configuration["JwtTokenSettings:JwtRegisteredClaimNamesSub"];
        var key = Encoding.UTF8.GetBytes(_configuration["JwtTokenSettings:SymmetricSecurityKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration.GetSection("JwtTokenSettings")["ValidIssuer"],
            Audience = _configuration.GetSection("JwtTokenSettings")["ValidAudience"],
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwtSub),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                //new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                //new Claim(ClaimTypes.Role, user.Role.ToString())
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
