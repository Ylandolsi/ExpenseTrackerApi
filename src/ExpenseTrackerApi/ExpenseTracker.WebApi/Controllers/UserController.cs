using System.Security.Claims;
using ExpenseTrackerApi.Authentication.Contracts;
using ExpenseTrackerApi.Dto.Expense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;

namespace ExpenseTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IAuthenticationService authenticationService, IUserService userService,
        ILogger<UserController> logger)
    {
        _authenticationService = authenticationService;
        _userService = userService;
        _logger = logger;
    }
    
    [HttpPost("CreateAccount")]
    public async Task<IActionResult> CreateAccount([FromBody] UserCreateDto userRegistration)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        await _userService.RegisterUserAsync(userRegistration);
        return Ok("User registered successfully");
    }

    [Authorize]
    [HttpGet("expense/all")]
    public async Task<IActionResult> GetAllExpenses()
    {
        var userId = int.Parse((User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));
        _logger.LogInformation("User Id : " + userId);
        return Ok(await _userService.GetAllExpenses(userId));
    }

    [Authorize]
    [HttpPost("expense/create")]
    public async Task<IActionResult> AddNewExpense([FromBody] ExpenseManipulationDto expense)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        var userId = int.Parse((User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));
        _logger.LogInformation("User Id : " + userId);
        await _userService.CreateExpenseAsync(expense, userId);
        return Ok("Expense created successfully");
    }

    [Authorize]
    [HttpPut("expense/{id}/update")]
    public async Task<IActionResult> UpdNewExpense(int id, [FromBody] ExpenseManipulationDto expenseManipulationDto)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        if (id <= 0)
        {
            return BadRequest("Invalid Id");
        }

        var userId = int.Parse((User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));
        _logger.LogInformation("User Id : " + userId);
        await _userService.UpdateExpenseAsync(expenseManipulationDto, id, userId);
        return Ok("Expense updated successfully");
    }

    [Authorize]
    [HttpDelete("expense/{id}/delete")]
    public async Task<IActionResult> DelNewExpense(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid Id");
        }

        var userId = int.Parse((User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));
        _logger.LogInformation("User Id : " + userId);
        await _userService.DeleteExpenseAsync(id, userId);
        return Ok("Expense deleted successfully");
    }

    [Authorize]
    [HttpGet("expense/{LastDays}")]
    public async Task<IActionResult> GetExpensesOfLast(int LastDays)
    {
        if (LastDays < 0)
        {
            return BadRequest("Invalid Value For LastDays : it should be positive");
        }

        var userId = int.Parse((User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));
        _logger.LogInformation("User Id : " + userId);
        var expResult = await _userService.GetExpensesOfLastDays(userId, LastDays);
        return Ok(expResult);
    }

    [Authorize]
    [HttpGet("expense/{from}/{to}")]
    // http://localhost:5135/api/User/expense/2025-01-20/2025-01-25
    public async Task<IActionResult> GetExpensesOfLast(string from, string to)
    {
        if (!DateOnly.TryParse(from, out var startDate) || !DateOnly.TryParse(to, out var endDate))
        {
            return BadRequest("Invalid Value For Dates : Dates should be in the format yyyy-MM-dd");
        }

        if (startDate > endDate)
        {
            return BadRequest("Invalid Value For Dates : startDate should be less than endDate");
        }

        var userId = int.Parse((User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"));
        _logger.LogInformation("User Id : " + userId);
        var expResult = await _userService.GetExpensesFromxToy(userId, startDate, endDate);
        return Ok(expResult);
    }
}
