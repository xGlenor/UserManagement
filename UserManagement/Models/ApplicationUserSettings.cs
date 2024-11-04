using System.ComponentModel;

namespace UserManagement.Models;

public class ApplicationUserSettings
{
    public int Id { get; set; }
    public string UserId { get; set; }
    
    [DefaultValue(30)]
    public int? PasswordExpirationDays { get; set; }
    
    [DefaultValue(60)]
    public int? SessionTimeout { get; set; }
}