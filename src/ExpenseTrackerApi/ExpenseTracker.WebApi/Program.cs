using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerApi.Configuration;
using ExpenseTrackerApi.ExceptionHandler;
using ExpenseTrackerApi.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedDb.DbContext;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // (problem : circular references when serializing the data ) 
        // Task have User and user Have task so we need to ignore the cycle
        
        // to avoid infinite loop !  ( replace the cycle with null and print the others ) 
        // or we can just use [JsonIgnore] on the navigation property ORRR Dto for repsponse
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        //--------------------

        // to avoid the camel case ( default ) :
        // keeps the property name as it is ( when converting the models to json) 
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 

    });

builder.ConfigureRedis();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions> ,ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureCors();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ensure issuer = valideIssuer
            ValidateIssuer = true,
            // ensure audience = valideAudience
            ValidateAudience = true,
            // ensure the token is not expired
            ValidateLifetime = true,
            //  Ensures that the token’s signing key is validated 
            // by using IssuerSigningKey below 
            ValidateIssuerSigningKey = true, 

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            // if we use custom claim name for anyclaim we should specify it here
            // ex : RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(); // enables role based authorization , claims based authorization .... 





builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<DbUpdateExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.ConfigureUserService();

builder.Services.AddProblemDetails(); // to return problem details in case of error ( Excepion Handlers ) 


builder.Services.AddAutoMapper(typeof(Program));
// to enable custoum response from action
// exp : return BadRequest("some message")
// cuz [apiController] return a default response ( 400 - badRequest ) 
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();
app.UseExceptionHandler();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExpenseTracker API ");
});


app.UseStaticFiles(); // Enable static files ( html , css , js , images .. )  to be served
app.UseRouting(); // maps incoming requests to route handlers

app.UseCors("CorsPolicy"); // allowing or blocking  requests from different origins ( cross-origin requests )

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
