namespace ExpenseTrackerApi;

public static class LoginMethods
{
    
    public static string HashPassword(string password)
    {
        // Implement password hashing logic
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    public static bool VerifyPasswordHash(string password, string storedHash)
    {
        // Implement password verification logic (e.g., using BCrypt)
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}
