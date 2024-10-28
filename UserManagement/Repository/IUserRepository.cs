using UserManagement.Models;

namespace UserManagement.Repository;

public interface IUserRepository
{
    Task<IEnumerable<ApplicationUser>> GetAllAsync();
    Task<ApplicationUser?> GetByIdAsync(string ID);

    Task UpdateAsync(ApplicationUser user);
    Task DeleteAsync(string ID);
    Task SaveAsync();
    
    
    Task UpdateLastLogin(string Id);
    
    Task<List<UserPasswordHistory>> UserPasswordHistory(string Id);
    Task CreatePasswordHistory(ApplicationUser user);

    Task CreateUserSettings(ApplicationUser user);
    Task<ApplicationUserSettings?> GetUserSettingsById(string Id);
    Task UpdateUserSettings(ApplicationUserSettings userSettings);
}