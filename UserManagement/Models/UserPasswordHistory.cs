namespace UserManagement.Models;

public class UserPasswordHistory
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string PasswordHash { get; set; }
    public DateTime DateSet { get; set; }
}
