using Microsoft.AspNetCore.Identity;
using UserManagement.Helper;
using UserManagement.Models;
using UserManagement.Repository;

namespace UserManagement.Data;

public static class AdminAndUserInitializer
{
    
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        
        using (var scope = serviceProvider.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            var admin = await UserHelper.CreateUser(userManager, new UserHelper.UserRecord("admin@test.com", "Admin", "Admin12#$"));
            if (admin != null)
            {
                await userRepository.CreateUserSettings(admin);
                var test= await UserHelper.AssingRoles(userManager, new UserHelper.UserRolesRecord(admin, new string[] { "Admin" }));
            }
            
            var user = await UserHelper.CreateUser(userManager, new UserHelper.UserRecord("user@test.com", "User", "User12#$"));
            if (user != null)
            {
                await userRepository.CreateUserSettings(user);
                var test=await UserHelper.AssingRoles(userManager, new UserHelper.UserRolesRecord(user, new string[] { "User" }));
            }
        }
    }
}