namespace RCP_Project.DTO;

public class RegisterRequest
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } // "Admin" or "Standard"
}