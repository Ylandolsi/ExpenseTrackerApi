using ExpenseTrackerApi.Authentication;
using ExpenseTrackerApi.Authentication.Contracts;
using ExpenseTrackerApi.Services;
using Microsoft.OpenApi.Models;

namespace ExpenseTrackerApi.Configuration;

public static class ServiceConfiguartion
{
    public static void ConfigureCors(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy(
                "CorsPolicy",
                builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );
        });

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "toDoList api ", Version = "v1" });
            c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Name = "Authorization", // name of header used to send the auth
                Type = SecuritySchemeType.Http, // use http based auth 
                Scheme = "basic",
                In = ParameterLocation.Header, // location of auth token ( http header )
                Description = "Basic Authorization header."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                // You'll see a lock icon  next to each endpoint.
                // Clicking the lock icon will prompt you to enter your username and password.
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "basic"
                        }
                    },
                    new string[] { }
                }
            });
        });
    }

    public static void ConfigureRefreshTokenService(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    }

    public static void ConfigureAuthService(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }

    public static void ConfigureUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
}
