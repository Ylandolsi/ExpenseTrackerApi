using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Dto.Expense;

public record ExpenseManipulationDto
{
    
    public string expenseDescription { get; set; } = "No description";
    [Required]
    public string expenseCategory { get; set; }
    [Required]
    public string expensePrice { get; set; }
    [Required]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly expenseDate { get; set; }

}
