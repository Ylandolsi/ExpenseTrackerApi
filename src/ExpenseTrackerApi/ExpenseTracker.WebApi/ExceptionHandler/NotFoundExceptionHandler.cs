using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Models.Exceptions;

namespace ExpenseTrackerApi.ExceptionHandler;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException)
            return false;

        var notFoundException = (NotFoundException)exception;
        _logger.LogError($"Something went wrong: {notFoundException.Message}");
        var problemDetails = new ProblemDetails() // problem details is a class injected 
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found ",
            Detail = notFoundException.Message
        };
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }
}
