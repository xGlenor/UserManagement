using UserManagement.Models;

namespace UserManagement.ViewModels;

public class UserWithRolesViewModel
{
    public ApplicationUser User { get; set; }
    public List<string> Roles { get; set; }
}