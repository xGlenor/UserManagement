using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using UserManagement.Models;
using UserManagement.Repository;
using UserManagement.ViewModels;

namespace UserManagement.Controllers;

public class UserController: Controller
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public UserController(IUserRepository userRepository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

[Authorize]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userWithRoles = new List<UserWithRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userWithRoles.Add(new UserWithRolesViewModel
            {
                User = user,
                Roles = roles.ToList()
            });
        }

        return View(userWithRoles);
    }

 
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationUser user, string password)
    {
        if (ModelState.IsValid)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userRepository.CreatePasswordHistory(user);
                await _userRepository.CreateUserSettings(user);
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(user);
    }

    public async Task<IActionResult> Modify(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        var userSettings = await _userRepository.GetUserSettingsById(user.Id);

        if (userSettings == null)
        {
            return NotFound();
        }
        return View(new UserViewModel() { User = user, UserSettings = userSettings});
    }

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modify(UserViewModel userModel, string newPassword, int passwordExpirationDays)
    {
        var existingUser = await _userRepository.GetByIdAsync(userModel.User.Id);
        var userSettings = await _userRepository.GetUserSettingsById(userModel.User.Id);
        if (existingUser == null || userSettings == null)
        {
            return NotFound();
        }


        existingUser.Email = userModel.User.Email;
        existingUser.UserName = userModel.User.UserName;
        existingUser.IsRestrictionsDisabled = userModel.User.IsRestrictionsDisabled;

        userSettings.PasswordExpirationDays = passwordExpirationDays;


        if (!string.IsNullOrEmpty(newPassword))
        {
            var removePasswordResult = await _userManager.RemovePasswordAsync(existingUser);
            if (!removePasswordResult.Succeeded)
            {
                foreach (var error in removePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(userModel);
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(existingUser, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(userModel);
            }
        }


        var updateResult = await _userManager.UpdateAsync(existingUser);
         await _userRepository.UpdateUserSettings(userSettings);
        if (updateResult.Succeeded)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in updateResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(userModel);
    }


    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }


    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            await _userRepository.DeleteAsync(id);
        }
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> Lock(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); 
        user.LockoutEnabled = true;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Error");
    }

    [HttpPost]
    public async Task<IActionResult> Unlock(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.LockoutEnd = null; 
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Error");
    }

    public async Task<IActionResult> AssignRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var roles = await _roleManager.Roles.ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new AssignRolesViewModel
        {
            User = user,
            Roles = roles.ToList(),
            UserRoles = userRoles.ToList()
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoles(string userId, List<string> userRoles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }


        userRoles ??= new List<string>();


        var currentRoles = await _userManager.GetRolesAsync(user);


        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(userRoles));


        var addResult = await _userManager.AddToRolesAsync(user, userRoles.Except(currentRoles));

        if (removeResult.Succeeded && addResult.Succeeded)
        {
            return RedirectToAction(nameof(Index)); 
        }

        foreach (var error in removeResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        foreach (var error in addResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }


        var roles = await _roleManager.Roles.ToListAsync();
        var model = new AssignRolesViewModel
        {
            User = user,
            Roles = roles.ToList(),
            UserRoles = currentRoles.ToList()
        };

        return View(model);
    }
    
    
    
}