using System.ComponentModel.DataAnnotations;

namespace Models.Entities;

public class AuthUser
{
    public int userId { get; set; } // primary  & foreign key
    [Required]
    public string hashedPassword { get; set; }
    [Required]
    public string email { get; set; }
    [Required]
    public string phoneNumber { get; set; }
    
    public UserProfile UserProfile { get; set; }
}
