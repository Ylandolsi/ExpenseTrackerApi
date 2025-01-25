using ExpenseTracker.IdentifiyApi.Authentication.Contracts;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.Entities.Dto.User;
namespace ExpenseTracker.IdentifiyApi.Controllers;

[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityController> _logger;
    private readonly IAuthenticationService _authenticationService;

    public IdentityController(IConfiguration configuration, ILogger<IdentityController> logger,
        IAuthenticationService authenticationService)
    {
        _configuration = configuration;
        _logger = logger;
        _authenticationService = authenticationService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] UserLogin loginDto)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        var userProps = await _authenticationService.AuthenticateUserAsync(loginDto);
        if (userProps == null)
        {
            return Unauthorized("Invalid email or password");
        }

        _logger.LogInformation("User Authenticated");
        return Ok(userProps);
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenBody token)
    {
        if (string.IsNullOrEmpty(token.refreshToken))
            return BadRequest("Token is required");
        var userProps = await _authenticationService.RefreshToken(token.refreshToken);
        _logger.LogInformation("Refresh Token Validated");
        return Ok(userProps);
    }


    [HttpPost("RevokeToken")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenBody token)
    {
        if (string.IsNullOrEmpty(token.refreshToken))
            return BadRequest("Token is required");
        await _authenticationService.RevokeToken(token.refreshToken);
        _logger.LogInformation("Token Revoked");
        return Ok("Token Revoked");
    }
}
