using JwtAuthApi.Enums;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthApi.Models;

public class ApplicationUser : IdentityUser
{
    public Role Role { get; set; }

    public string? Hobby {  get; set; }

//    public string? RefreshToken { get; set; }
//    public DateTime RefreshTokenExpiryTime { get; set; }
}