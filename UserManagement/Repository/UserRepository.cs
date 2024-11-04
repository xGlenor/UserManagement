using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Helper;
using UserManagement.Models;
using UserManagement.Services;
using LogHelper = Microsoft.IdentityModel.Logging.LogHelper;

namespace UserManagement.Repository;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _appDbContext;
    private readonly ILogService _logService;

    public UserRepository(UserManager<ApplicationUser> userManager, AppDbContext appDbContext, ILogService logService)
    {
        _userManager = userManager;
        _appDbContext = appDbContext;
        this._logService = logService;
    }


    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users.ToListAsync();
    }


    public async Task<ApplicationUser?> GetByIdAsync(string ID)
    {
        return await _userManager.FindByIdAsync(ID);
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        await _userManager.UpdateAsync(user);
    }

    public async Task DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            var deletedResult = await _userManager.DeleteAsync(user);
            if (deletedResult.Succeeded)
            {
                
            }
        }
    }

    public async Task SaveAsync()
    {
       
    }

    public async Task UpdateLastLogin(string Id)
    {
        var user = await _userManager.FindByIdAsync(Id);
        if (user != null)
        {
            user.LastLogin = DateTime.Now;
            var updatetd = await _userManager.UpdateAsync(user);
            
        }
    }

    public Task<List<UserPasswordHistory>> UserPasswordHistory(string Id)
    {
        return _appDbContext.UserPasswordHistories
            .Where(x => x.UserId == Id)
            .OrderByDescending(x => x.DateSet)
            .Take(3)
            .ToListAsync();
    }

    public async Task CreatePasswordHistory(ApplicationUser user)
    {
        var userPassword = new UserPasswordHistory()
        {
            PasswordHash = user.PasswordHash,
            DateSet = DateTime.Now,
            UserId = user.Id,
            
        };
        await _appDbContext.UserPasswordHistories.AddAsync(userPassword);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task CreateUserSettings(ApplicationUser user)
    {
        var userSettings = new ApplicationUserSettings()
        {
            UserId = user.Id,
            PasswordExpirationDays = 30
        };

        await _appDbContext.UserSettings.AddAsync(userSettings);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<ApplicationUserSettings?> GetUserSettingsById(string Id)
    {
        var userSettings = await _appDbContext.UserSettings.FirstOrDefaultAsync(us => us.UserId.Equals(Id));
        if (userSettings != null) return userSettings;
        return null;
    }

    public async Task UpdateUserSettings(ApplicationUserSettings userSettings)
    {
         _appDbContext.UserSettings.Update(userSettings);
         await _appDbContext.SaveChangesAsync();
    }
}