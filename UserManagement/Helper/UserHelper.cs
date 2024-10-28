using Microsoft.AspNetCore.Identity;
using UserManagement.Models;

namespace UserManagement.Helper;

public class UserHelper
{
    public record UserRecord(string Email, string UserName, string Password);
    public record UserRolesRecord(ApplicationUser User, string[] Roles);
    
    public static async Task<ApplicationUser?> CreateUser(UserManager<ApplicationUser> userManager, UserRecord userRecord)
    {
        var user = await userManager.FindByEmailAsync(userRecord.Email);

        if (user == null)
        {
            var newUser = new ApplicationUser
            {
                Email = userRecord.Email,
                UserName = userRecord.UserName,
                EmailConfirmed = true,
                LastLogin = DateTime.Now.AddDays(-31),
                IsRestrictionsDisabled = false
            };
            
            var result = await userManager.CreateAsync(newUser, userRecord.Password);
            if (result.Succeeded)
            {
                
                return newUser;
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                }
            }
        }
        return null;
    }

    public static async Task<bool> AssingRoles(UserManager<ApplicationUser> userManager, UserRolesRecord rolesRecord)
    {
     
        var user = await userManager.FindByEmailAsync(rolesRecord.User.Email);

        if (user != null)
        {
            var result = await userManager.AddToRolesAsync(user, rolesRecord.Roles);

            if (result.Succeeded)
            {
                return true;
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                }
            }
        }

        return false;
    }

    public static int GetLastLoginDays(DateTime lastLogin)
    {
        return (int)(DateTime.Now - lastLogin).TotalDays;
    }
}