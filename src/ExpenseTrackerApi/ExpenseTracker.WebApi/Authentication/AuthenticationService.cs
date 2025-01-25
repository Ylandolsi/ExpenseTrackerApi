using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTrackerApi.Authentication.Contracts;
using ExpenseTrackerApi.DbContext;
using ExpenseTrackerApi.Dto.Expense;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.Entities;
using Models.Exceptions;

namespace ExpenseTrackerApi.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IRefreshTokenService _tokenService;

    public AuthenticationService(IConfiguration configuration
        , ILogger<AuthenticationService> logger,
        ApplicationDbContext dbContext,
        IRefreshTokenService tokenService)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<object> AuthenticateUserAsync(UserLogin loginRequest)
    {
        if (loginRequest.email is null && loginRequest.phoneNumber is null)
        {
            throw new BadRequestException("Email or phone number is required.");
        }


        // Authenticate the user by email or password.
// Step 1: Fetch the user from the database based on email or phone number
        var authUser = await _dbContext.AuthUsers
            .Include(u => u.UserProfile).FirstOrDefaultAsync(u =>
                (loginRequest.email != null && u.email.ToLower() == loginRequest.email.ToLower()) ||
                (loginRequest.phoneNumber != null && u.phoneNumber.ToLower() == loginRequest.phoneNumber.ToLower())
            );

        if (authUser == null)
        {
            // Handle the case where no matching user is found
            throw new Exception("Invalid email/phone number.");
        }

// Step 2: Verify the password hash in memory
        if (!LoginMethods.VerifyPasswordHash(loginRequest.Password, authUser.hashedPassword))
        {
            // Handle the case where the password is incorrect
            throw new BadRequestException("Invalid password.");
        }

        // Generate a new refresh token.
        var refreshToken = GenerateRefreshToken();

        // Save the new refresh token with associated user information.
        await _tokenService.SaveRefreshToken(authUser.userId, refreshToken);
        var token = IssueAccessToken(authUser);

        return new { AccessToken = token, RefreshToken = refreshToken };
    }

    // Refreshes an access token using a valid refresh token.
    public async Task<object> RefreshToken(string request)
    {
        // Retrieve the username associated with the provided refresh token.
        var idUser = await _tokenService.RetrieveUsernameByRefreshToken(request) ?? 0;

        if (idUser == 0)
        {
            throw new BadRequestException("Invalid refresh token.");
        }

        // Retrieve the user by username.
        var user = _dbContext.AuthUsers
            .Include(u => u.UserProfile)
            .FirstOrDefault(u => u.userId == idUser);
        if (user == null)
        {
            return null;
        }

        // Issue a new access token and refresh token for the user.
        var accessToken = IssueAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Save the new refresh token.
        await _tokenService.SaveRefreshToken(idUser, newRefreshToken);

        // Return the new access and refresh tokens.
        return new { AccessToken = accessToken, RefreshToken = newRefreshToken };
    }


    // Revokes a refresh token to prevent its future use. ( delete ) 
    public async Task RevokeToken(string request)
    {
        // Attempt to revoke the refresh token.
        var result = await _tokenService.RevokeRefreshToken(request);
        if (!result)
        {
            throw new NotFoundException("Refresh token not found."); // Return not found if the token does not exist.
        }
    }


    // Private method to generate a JWT token using the user's data.
    private string IssueAccessToken(AuthUser user)
    {
        // Creates a new symmetric security key from the JWT key specified in the app configuration.
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        // Sets up the signing credentials using the above security key and specifying the HMAC SHA256 algorithm.
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Defines a set of claims to be included in the token.
        var claims = new List<Claim>
        {
            // Standard JWT claim for subject, using user ID.
            new Claim(ClaimTypes.NameIdentifier, user.userId.ToString()),
            // Standard claim for user identifier, using username.
            new Claim(ClaimTypes.Name, user.UserProfile.Name),
            // Standard claim for user's email.
            new Claim(ClaimTypes.Email, user.email)
        };
        // Add role claims for the user.
        // user.Roles.ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));


        // Creates a new JWT token with specified parameters including issuer, audience, claims, expiration time, and signing credentials.
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1), // Token expiration set to 1 hour from the current time.
            signingCredentials: credentials);

        // Serializes the JWT token to a string and returns it.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generates a new refresh token using a cryptographic random number generator.
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32]; // Prepare a buffer to hold the random bytes.
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber); // Fill the buffer with cryptographically strong random bytes.
            return Convert.ToBase64String(randomNumber); // Convert the bytes to a Base64 string and return.
        }
    }
}
