using JwtAuthApi.Models;

namespace JwtAuthApi.Services;

public interface IUserService
{
    public string Login(User user);
}
