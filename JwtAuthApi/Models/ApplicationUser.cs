using JwtAuthApi.Enums;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthApi.Models;

public class ApplicationUser : IdentityUser
{
    public Role Role { get; set; }

    public string? Hobby {  get; set; } 
}