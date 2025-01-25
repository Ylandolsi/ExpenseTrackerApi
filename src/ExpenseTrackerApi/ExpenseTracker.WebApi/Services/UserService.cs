using AutoMapper;
using ExpenseTrackerApi.Authentication.Contracts;
using ExpenseTrackerApi.DbContext;
using ExpenseTrackerApi.Dto.Expense;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Models.Exceptions;

namespace ExpenseTrackerApi.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private int idclient = 1;
    private readonly IMapper _mapper;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task RegisterUserAsync(UserCreateDto userRegistration)
    {
        _logger.LogInformation("Registering user");
        _logger.LogInformation("mapping the DTOs ");
        var user = _mapper.Map<UserProfile>(userRegistration);
        var userAuth = _mapper.Map<AuthUser>(userRegistration);

        userAuth.UserProfile = user;
        await _context.UserProfiles.AddAsync(user);
        await _context.AuthUsers.AddAsync(userAuth);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User registered successfully");
    }

    public async Task CreateExpenseAsync(ExpenseManipulationDto expenseCreateDto, int userId)
    {
        _logger.LogInformation("Creating expense");
        _logger.LogInformation("Mapping the DTO");
        var expense = _mapper.Map<Expense>(expenseCreateDto);

        expense.userProfilesId = userId;
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Expense created successfully");
    }

    public async Task UpdateExpenseAsync(ExpenseManipulationDto expenseManipulationDto, int expenseid, int userId)
    {
        _logger.LogInformation("Updating expense");
        _logger.LogInformation($"Searching for expense with id = {expenseid}");
        var expense =
            await _context.Expenses.FirstOrDefaultAsync(e => e.expenseId == expenseid && e.userProfilesId == userId);
        if (expense is null)
        {
            _logger.LogError($"Expense with id = {expenseid} not found");
            throw new NotFoundException($"Expense with id = {expenseid} not found");
        }

        _mapper.Map(expenseManipulationDto, expense);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Expense updated successfully");
    }

    public async Task DeleteExpenseAsync(int expenseid, int userId)
    {
        _logger.LogInformation("Deleting expense");
        _logger.LogInformation($"Searching for expense with id = {expenseid}");
        var expense =
            await _context.Expenses.FirstOrDefaultAsync(e => e.expenseId == expenseid && e.userProfilesId == userId);
        if (expense is null)
        {
            _logger.LogError($"Expense with id = {expenseid} not found");
            throw new NotFoundException($"Expense with id = {expenseid} not found");
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Expense Deleted successfully");
    }

    public async Task<IEnumerable<ExpenseManipulationDto>> GetExpensesOfLastDays(int userId, int lastDays)
    {
        _logger.LogInformation($"Searching for expenses of last {lastDays} days");
        var endDate = DateOnly.FromDateTime(DateTime.Now);
        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-lastDays));
        _logger.LogInformation("Calling the GetExpensesFromxToy method");
        return await GetExpensesFromxToy(userId, startDate, DateOnly.FromDateTime(DateTime.Now));
    }

    public async Task<IEnumerable<ExpenseManipulationDto>> GetExpensesFromxToy(int userId, DateOnly start, DateOnly end)
    {
        _logger.LogInformation($"Getting expenses from day {start} to day {end}");
        var expenses = await _context.Expenses
            .Where(e => e.userProfilesId == userId && e.expenseDate >= start && e.expenseDate <= end)
            .ToListAsync();
        expenses ??= new List<Expense>();
        _logger.LogInformation("Mapping the DTOs");
        return _mapper.Map<IEnumerable<ExpenseManipulationDto>>(expenses);
    }

    public async Task<IEnumerable<ExpenseManipulationDto>> GetAllExpenses(int userId)
    {
        _logger.LogInformation("Getting all expenses");
        var expenses = await _context.Expenses
            .Where(e => e.userProfilesId == userId)
            .ToListAsync();
        expenses ??= new List<Expense>();

        _logger.LogInformation("Mapping the DTOs");
        return _mapper.Map<IEnumerable<ExpenseManipulationDto>>(expenses);
    }
}
