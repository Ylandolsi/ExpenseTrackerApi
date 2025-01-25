using System.ComponentModel.DataAnnotations;

namespace Models.Entities;

public class Expense
{
    public int expenseId { get; set; }
    
    public string expenseDescription { get; set; } = "No description";
    [Required]
    public string expenseCategory { get; set; }
    [Required]
    public string expensePrice { get; set; }
    [Required]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly expenseDate { get; set; }
    
    // foreign key 
    public int? userProfilesId { get; set; }
    public UserProfile UserProfile { get; set; }
}
