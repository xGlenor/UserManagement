using Microsoft.AspNetCore.Identity;

namespace UserManagement.Models;

public class ApplicationUser: IdentityUser
{
    public DateTime? LastLogin { get; set; }
    public bool IsRestrictionsDisabled { get; set; }
}