using UserManagement.Models;

namespace UserManagement.ViewModels;

public class UserViewModel
{
    public ApplicationUser User { get; set; }
    public ApplicationUserSettings UserSettings { get; set; }
}