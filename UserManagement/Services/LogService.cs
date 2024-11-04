using UserManagement.Models;
using UserManagement.Repository;

namespace UserManagement.Services;

public interface ILogService
{
    void CreateLog(ApplicationUser user, string action, string status, string message);
    Task<ICollection<Logg>> GetLogs();
    Task<ICollection<Logg>> GetLogsByAction(string action);
    Task<ICollection<Logg>> GetLogsByUserId(string id);
    Task<ICollection<Logg>> GetLogsByUserName(string username);
    Task<Logg> GetLogById(int id);

};

public class LogService(ILoggRepository repo) : ILogService
{
    public async void CreateLog(ApplicationUser user, string action, string status, string message)
    {
        await repo.CreateLog(new Logg()
        {
            UserName = user.UserName ?? user.Id,
            UserId = user.Id,
            Action = action,
            Status = status,
            Message = message,
            AtCreated = DateTime.Now
        });
    }

    public async Task<ICollection<Logg>> GetLogs()
        => await repo.GetLogs();

    public async Task<ICollection<Logg>> GetLogsByAction(string action)
        => await repo.GetLogsByAction(action.ToLower());
    
    public async Task<ICollection<Logg>> GetLogsByUserId(string id)
        => await repo.GetLogsByUserId(id);

    public async Task<ICollection<Logg>> GetLogsByUserName(string username)
        => await repo.GetLogsByUserName(username);

    public async Task<Logg> GetLogById(int id)
        => await repo.GetLogById(id);

}