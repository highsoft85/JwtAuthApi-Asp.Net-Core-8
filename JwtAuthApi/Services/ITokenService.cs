using JwtAuthApi.Models;
using System.Security.Claims;

namespace JwtAuthApi.Services;

public interface ITokenService
{
    public string CreateToken(ApplicationUser user);

//    string GenerateRefreshToken();
//    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
