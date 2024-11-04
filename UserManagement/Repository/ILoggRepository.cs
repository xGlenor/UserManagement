using UserManagement.Models;

namespace UserManagement.Repository;

public interface ILoggRepository
{
    Task<ICollection<Logg>> GetLogs();
    Task<ICollection<Logg>> GetLogsByAction(string action);
    Task<ICollection<Logg>> GetLogsByUserId(string id);
    Task<ICollection<Logg>> GetLogsByUserName(string username);
    Task<Logg> GetLogById(int Id);
    
    Task CreateLog(Logg logg);
}