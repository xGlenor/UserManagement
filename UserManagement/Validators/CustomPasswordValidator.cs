using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Validators;

public class CustomPasswordValidator : PasswordValidator<ApplicationUser>
{
    private readonly AppDbContext _context;

    public CustomPasswordValidator(AppDbContext context)
    {
        _context = context;
    }

    public override async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
    {

        if (HasRepeatingCharacters(password) && !user.IsRestrictionsDisabled)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Hasło nie może zawierać powtarzających się znaków."
            });
        }
        
        
        var passwordHistory = await _context.UserPasswordHistories
            .Where(ph => ph.UserId == user.Id)
            .OrderByDescending(ph => ph.DateSet)
            .Take(3)
            .ToListAsync();

        foreach (var history in passwordHistory)
        {
            if (manager.PasswordHasher.VerifyHashedPassword(user, history.PasswordHash, password) == PasswordVerificationResult.Success)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordUsedRecently",
                    Description = "Nie możesz użyć jednego z trzech ostatnich haseł."
                });
            }
        }

       
        return await base.ValidateAsync(manager, user, password);
    }
    private bool HasRepeatingCharacters(string password)
    {
        return password.GroupBy(c => c).Any(g => g.Count() > 1);
    }
}