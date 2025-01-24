using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApi.ExceptionHandler;

public class DbUpdateExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DbUpdateExceptionHandler> _logger;

    public DbUpdateExceptionHandler(ILogger<DbUpdateExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException)
            return false;

        var dbUpdateException = (DbUpdateException)exception;
        _logger.LogError($"Something went wrong: {dbUpdateException.Message}");
        var problemDetails = new ProblemDetails() // problem details is a class injected 
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Bad Request",
            Detail = dbUpdateException.Message
        };
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }
}
