namespace ExpenseTrackerApi.Dto.Expense;

public record UserCreateDto
{
    public string email { get; init; }
    public string phoneNumber { get; init; }
    public string Password { get; init; }
    public string Name { get; init; }
    
}
