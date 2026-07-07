using AuthService_SB.Domain.Entities;
using AuthService_SB.Application.Services;
using AuthService_SB.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService_SB.Persistence.Data;

public static class DataSeeder
{
    // Roles must exist in every environment for role-based auth to work, but a
    // default admin account with a guessable username==password must never be
    // auto-created outside local development.
    public static async Task SeedAsync(ApplicationDbContext context, bool isDevelopment, ILogger? logger = null)
    {
        if(!context.Roles.Any())
        {
            var roles = new List<Role>
            {
                new()
                {
                    Id = UuidGenerator.GenerateRoleId(),
                        Name = RoleConstants.ADMIN_ROLE
                },
                new()
                {
                    Id = UuidGenerator.GenerateRoleId(),
                        Name = RoleConstants.USER_ROLE
                }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        if(!await context.Users.AnyAsync())
        {
            if (!isDevelopment)
            {
                logger?.LogWarning(
                    "No users exist and this is not a Development environment: skipping default admin seed. " +
                    "Create the first admin account explicitly (e.g. via a one-time admin CLI/migration step).");
                return;
            }

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.ADMIN_ROLE);
            if(adminRole != null)
            {
                var passwordHasher = new PasswordHashService();
                var profileId = UuidGenerator.GenerateUserId();
                var emailId = UuidGenerator.GenerateUserId();
                var userRoleId = UuidGenerator.GenerateUserId();
                var userId = UuidGenerator.GenerateUserId();

                var adminUser = new User
                {
                    Id = userId,
                    Name = "Admin",
                    Surname =  "User",
                    Username = "ADMINB",
                    Email = "admin@ksports.local",
                    Password = passwordHasher.HashPassword("ADMINB"),
                    Status = true,
                    UserProfile = new UserProfile
                    {
                        Id = profileId,
                        UserId = userId,
                        ProfilePicture = string.Empty,
                        Phone = string.Empty
                    },
                    UserEmail = new UserEmail
                    {
                        Id = emailId,
                        UserId = userId,
                        EmailVerified = true,
                        EmailVerificationToken = null,
                        EmailVerificationTokenExpiry = null
                    },
                    UserRoles =
                    [
                        new UserRole
                        {
                            Id = userRoleId,
                            UserId = userId,
                            RoleId = adminRole.Id
                        }
                    ]
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}