namespace AuthDemo.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";        // stored lowercase, unique
    public string PasswordHash { get; set; } = ""; // BCrypt, never the raw password
    public string Role { get; set; } = "User";     // used for authorization demo
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
