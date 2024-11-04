using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;
using UserManagement.Repository;
using UserManagement.Services;
using UserManagement.ViewModels;

namespace UserManagement.Controllers;

[Authorize]
public class UserController: Controller
{
    private readonly IUserRepository _userRepository;
    private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
    private readonly Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> _roleManager;
    private readonly ILogService _logService;
    
    public UserController(IUserRepository userRepository, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleManager, ILogService logService)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _logService = logService;
    }

    
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
                _logService.CreateLog(GetUser(),"CREATE USER", "SUCCESS", $"User Created '{user.UserName}'");
                return RedirectToAction(nameof(Index));
            }
            var errors = string.Join($"User: {user.UserName ?? user.Id }\n", result.Errors.Select(e => e.Description));
            _logService.CreateLog(GetUser(), "CREATE USER", "ERROR", errors);
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
        
        var user = userModel.User;
        
        if (existingUser == null || userSettings == null)
        {
            _logService.CreateLog(GetUser(), "UPDATE USER", "ERROR", "User or user settings not found");
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
                var errors = string.Join($"User: {user.UserName ?? user.Id }\n", removePasswordResult.Errors.Select(e => e.Description));
                _logService.CreateLog(GetUser(), "UPDATE USER PASSWORD", "ERROR", errors);
                return View(userModel);
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(existingUser, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {

                    ModelState.AddModelError(string.Empty, error.Description);
                }
                var errors = string.Join($"User: {user.UserName ?? user.Id }\n", addPasswordResult.Errors.Select(e => e.Description));
                _logService.CreateLog(GetUser(), "UPDATE USER PASSWORD", "ERROR", errors);
                return View(userModel);
            }
        }


        var updateResult = await _userManager.UpdateAsync(existingUser);
         await _userRepository.UpdateUserSettings(userSettings);
        if (updateResult.Succeeded)
        {
            _logService.CreateLog(GetUser(), "UPDATE USER", "SUCCESS", "User updated successfully");
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in updateResult.Errors)
        {

            ModelState.AddModelError(string.Empty, error.Description);
        }
        var errorsUpdated = string.Join($"User: {user.UserName ?? user.Id }\n", updateResult.Errors.Select(e => e.Description));
        _logService.CreateLog(GetUser(), "UPDATE USER PASSWORD", "ERROR", errorsUpdated);
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
            _logService.CreateLog(GetUser(), "DELETE USER", "SUCCESS", "User deleted successfully");
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
            _logService.CreateLog(GetUser(), "LOCK USER", "SUCCESS", "User locked successfully");
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        var errors = string.Join($"User: {user.UserName ?? user.Id }\n", result.Errors.Select(e => e.Description));
        _logService.CreateLog(GetUser(), "LOCK USER", "ERROR", errors);
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
            _logService.CreateLog(GetUser(), "UNLOCK USER", "SUCCESS", "User unlocked successfully");
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        var errors = string.Join($"User: {user.UserName ?? user.Id }\n", result.Errors.Select(e => e.Description));
        _logService.CreateLog(GetUser(), "UNLOCK USER", "ERROR", errors);
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
    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Logs()
    {
        var logs = _logService.GetLogs();
        

        return View(logs);
    }

    public async Task<PartialViewResult> SearchLogs(string searchText)
    {
        ICollection<Logg> logs = GetLogs();

        if(string.IsNullOrEmpty(searchText))
            return PartialView("_GridView", logs);
        
        var result = logs
            .Where(u => 
                u.UserName.ToLower().Contains(searchText.ToLower()) ||
                u.UserId.ToLower().Contains(searchText.ToLower()) ||
                u.Status.ToLower().Contains(searchText.ToLower()) ||
                u.Action.ToLower().Contains(searchText.ToLower()) ||
                u.Message.ToLower().Contains(searchText.ToLower()) ||
                u.AtCreated.Value.ToString().Contains(searchText.ToLower())
                ).ToList();

        return PartialView("_GridView", result);
    }

    public List<Logg> GetLogs()
    {
        var logs = _logService.GetLogs().Result;
        
        return logs.ToList();
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
            var rolesAssigned = string.Join(',', userRoles);
            _logService.CreateLog(GetUser(), "ASSIGN ROLES", "SUCCESS", $"User assigned roles for {user.UserName} Roles: {rolesAssigned}");
            return RedirectToAction(nameof(Index)); 
        }

        var errorss = string.Join($"User: {user.UserName ?? user.Id }\nBefore Assign\n", removeResult.Errors.Select(e => e.Description));
        _logService.CreateLog(GetUser(), "ASSIGN ROLES", "ERROR", errorss);
        foreach (var error in removeResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        var errors = string.Join($"User: {user.UserName ?? user.Id }\n", addResult.Errors.Select(e => e.Description));
        _logService.CreateLog(GetUser(), "ASSIGN ROLES", "ERROR", errors);
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

    private ApplicationUser GetUser()
    {
        var userId = User.Identity.GetUserId();
        var getUser = _userRepository.GetByIdAsync(userId);
        if  (getUser.Result == null)
            return null;
        return getUser.Result;
        
    }
    
}