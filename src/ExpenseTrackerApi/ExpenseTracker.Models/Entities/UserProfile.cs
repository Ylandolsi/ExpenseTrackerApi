using System.ComponentModel.DataAnnotations;

namespace Models.Entities;

public class UserProfile
{
    public int userId { get; set; }
    [Required( ErrorMessage = "Name is required")]
    [Length(4 ,15 , ErrorMessage = "Name must be between 4 and 15 characters")]
    public string Name { get; set; }
    
    // Intialize the collection to avoid null reference exception
    public ICollection<Expense> UserExpenses { get; set; } = new List<Expense>();
    public AuthUser? AuthUser { get; set; }
    
}
