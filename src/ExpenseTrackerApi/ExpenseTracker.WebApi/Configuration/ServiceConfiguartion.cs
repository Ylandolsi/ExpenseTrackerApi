
using ExpenseTrackerApi.Services;

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
