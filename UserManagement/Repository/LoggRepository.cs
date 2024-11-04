using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Repository;

public class LoggRepository : ILoggRepository
{
    private readonly AppDbContext _context;

    public LoggRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<ICollection<Logg>> GetLogs()
    {
        return await _context.Loggs.ToListAsync();
    }

    public async Task<ICollection<Logg>> GetLogsByAction(string action)
    {
        return await _context.Loggs.Where(log => log.Action.ToLower().Equals(action.ToLower())).ToListAsync();
    }

    public async Task<ICollection<Logg>> GetLogsByUserId(string id)
    {
        return await _context.Loggs.Where(log => log.UserId.Equals(id)).ToListAsync();
    }

    public async Task<ICollection<Logg>> GetLogsByUserName(string username)
    {
        return await _context.Loggs.Where(log => log.UserName.Equals(username)).ToListAsync();
    }

    public async Task<Logg> GetLogById(int Id)
    {
        return await _context.Loggs.FirstOrDefaultAsync(log => log.Id.Equals(Id));
    }

    public async Task CreateLog(Logg logg)
    {
        await _context.Loggs.AddAsync(logg);
        await _context.SaveChangesAsync();
    }
}