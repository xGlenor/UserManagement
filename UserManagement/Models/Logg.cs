namespace UserManagement.Models;

public class Logg
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? UserId { get; set; }
    public string? Status { get; set; }
    public string? Action { get; set; }
    public string? Message { get; set; }
    public DateTime? AtCreated { get; set; }
}