using Models.Entities.Dto.User;
namespace ExpenseTracker.IdentifiyApi.Authentication.Contracts;

public interface IAuthenticationService
{
    Task<object> AuthenticateUserAsync(UserLogin loginRequest);

    // Refreshes an access token using a valid refresh token.
    Task<object> RefreshToken(string request);

    // delete a refresh token to prevent its future use. 
    Task RevokeToken(string request);
}
