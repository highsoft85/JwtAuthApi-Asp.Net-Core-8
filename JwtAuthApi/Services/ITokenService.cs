using JwtAuthApi.Models;
using System.Security.Claims;

namespace JwtAuthApi.Services;

public interface ITokenService
{
    public string CreateToken(ApplicationUser user);

    public string CreateRefreshToken();

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    public Object? GetConfigurationValue(string key);
}
