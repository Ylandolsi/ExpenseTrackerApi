using ExpenseTrackerApi.Authentication;
using ExpenseTrackerApi.Authentication.Contracts;
using ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

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

            // Add JWT Authentication support to Swagger
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // Must be lowercase
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
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

    public static void ConfigureRedis(this WebApplicationBuilder builder)
    {
        // Install-Package Microsoft.Extensions.Caching.StackExchangeRedis

        // Register the Redis connection multiplexer as a singleton service
        // This allows the application to interact directly with Redis for advanced scenarios
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            // Establish a connection to the Redis server using the configuration from appsettings.json
            ConnectionMultiplexer.Connect(builder.Configuration["RedisCacheOptions:Configuration"]));
    }
}
