namespace ExpenseTrackerApi.Authentication.Contracts;

public interface IRefreshTokenService
{
    Task SaveRefreshToken(int username, string token);

    Task<bool> RevokeRefreshToken(string refreshToken);
    Task<int?> RetrieveUsernameByRefreshToken(string refreshToken);
    
}
