using JwtAuthApi.Models;

namespace JwtAuthApi.Services;

public interface IUserService
{
    public string CreateToken(ApplicationUser user);
}
