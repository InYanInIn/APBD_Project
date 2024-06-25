using System.ComponentModel.DataAnnotations;

namespace RCP_Project.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public string Role { get; set; } // "Admin" or "Standard"
    public string RefreshToken { get; set; }
    public DateTime? RefreshTokenExp { get; set; }
}