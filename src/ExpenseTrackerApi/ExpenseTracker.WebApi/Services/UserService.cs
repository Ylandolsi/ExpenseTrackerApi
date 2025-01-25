using System.Text.Json;
using AutoMapper;
using ExpenseTrackerApi.Authentication.Contracts;
using ExpenseTrackerApi.DbContext;
using ExpenseTrackerApi.Dto.Expense;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Models.Exceptions;
using StackExchange.Redis;

namespace ExpenseTrackerApi.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _databaseCache;
    private readonly IConfiguration _configuration;


    public UserService(ApplicationDbContext context, ILogger<UserService> logger,
        IMapper mapper,
        IConnectionMultiplexer connectionMultiplexer,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _redisConnection = connectionMultiplexer;
        _databaseCache = _redisConnection.GetDatabase();
        _configuration = configuration;
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
        var cacheKey = $"Expenses_{userId}_{start.ToString()}_to_{end.ToString()}";
        _logger.LogInformation(cacheKey);
        
        var cacheValue = await _databaseCache.StringGetAsync(cacheKey);
        var slidingExpiration = TimeSpan.FromMinutes(1);


        if (cacheValue.HasValue)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);

            // Apply sliding expiration
            var remainingTime = await _databaseCache.KeyTimeToLiveAsync(cacheKey);

            if (remainingTime == null || slidingExpiration > remainingTime)
            {
                await _databaseCache.KeyExpireAsync(cacheKey, slidingExpiration);
                _logger.LogInformation("Updated sliding expiration for key: {CacheKey}", cacheKey);
            }

            // Deserialize the cached value
            return JsonSerializer.Deserialize<IEnumerable<ExpenseManipulationDto>>(cacheValue.ToString());
        }

        _logger.LogInformation($"Getting expenses from day {start} to day {end}");

        
        var expenses = await _context.Expenses
            .Where(e => e.userProfilesId == userId && e.expenseDate >= start && e.expenseDate <= end)
            .ToListAsync();
        expenses ??= new List<Expense>();
        
        
        _logger.LogInformation("Mapping the DTOs");
        
        var expenseDtos = _mapper.Map<IEnumerable<ExpenseManipulationDto>>(expenses);

        
        // Serialize and cache the result
        var serializedExpenses = JsonSerializer.Serialize(expenseDtos);
        await _databaseCache.StringSetAsync(cacheKey, serializedExpenses, slidingExpiration);

        _logger.LogInformation("Cached data for key: {CacheKey}", cacheKey);
        return expenseDtos;
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


    public Task DeleteAllCaches()
    {
        // Get the Redis server instance from the connection multiplexer
        var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

        // Iterate over all keys in the Redis database
        foreach (var key in server.Keys())
        {
            // Get the InstanceName from the configuration (as specified in appsettings.json)
            string instanceName = _configuration["RedisCacheOptions:InstanceName"] ?? string.Empty;

            // Remove the instance name prefix if it exists
            var keyWithoutPrefix = key.ToString().Replace($"{instanceName}", "");

            // Remove each key-value pair from the distributed cache
            // _distributedCache.Remove(keyWithoutPrefix);
            _databaseCache.KeyDelete(keyWithoutPrefix);
        }

        return Task.CompletedTask;
    }

    public async Task DeleteCache(string key)
    {
        // var cacheValue = await _distributedCache.GetStringAsync(key);
        var cacheValue = (await _databaseCache.StringGetAsync(key)).ToString();

        if (cacheValue is null)
        {
            _logger.LogInformation($"Deletion failed : Cache key {key} not found");

            return;
        }

        // Remove the specified key-value pair from the distributed cache
        // _distributedCache.Remove(key);
        _databaseCache.KeyDelete(key);
        _logger.LogInformation($"Cache key {key} deleted");
    }
}
