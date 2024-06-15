using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using JwtAuthApi.Models;
using Microsoft.AspNetCore.Identity;
using JwtAuthApi.Enums;

namespace JwtAuthApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Page> Pages => Set<Page>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed AspNetUsers table with default admin user
        var hasher = new PasswordHasher<ApplicationUser>();

        var adminName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("SiteSettings")["AdminUserName"];
        var adminEmail = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("SiteSettings")["AdminEmail"];
        var adminPassword = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("SiteSettings")["AdminPassword"];

        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                //Id = "1", // primary key
                UserName = adminName,
                NormalizedUserName = adminEmail?.ToUpper(),
                PasswordHash = hasher.HashPassword(null, adminPassword!),
                Email = adminEmail,
                NormalizedEmail = adminEmail?.ToUpper(),
                Role = Role.Admin
            }
        );
    }
}
