using Microsoft.AspNetCore.Identity;
using UserManagement.Models;

namespace UserManagement.ViewModels;

public class AssignRolesViewModel
{
    public ApplicationUser User { get; set; }
    public List<IdentityRole> Roles { get; set; }
    public List<string> UserRoles { get; set; } 
}