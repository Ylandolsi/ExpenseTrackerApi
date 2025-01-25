using Models.Entities.Dto.Expense;
using Models.Entities.Dto.User;

namespace ExpenseTrackerApi.Services;

public interface IUserService
{
    Task RegisterUserAsync(UserCreateDto userRegistration);
    Task CreateExpenseAsync(ExpenseManipulationDto expense, int userId );
    Task <IEnumerable<ExpenseManipulationDto>> GetAllExpenses(int userId);
    Task UpdateExpenseAsync(ExpenseManipulationDto expenseManipulationDto, int expenseid, int userId);
    Task DeleteExpenseAsync(int expenseid ,  int userId );
    
    Task<IEnumerable<ExpenseManipulationDto>> GetExpensesOfLastDays(int userId, int lastDays);
    Task<IEnumerable<ExpenseManipulationDto>> GetExpensesFromxToy(int userId, DateOnly start, DateOnly end);






}
