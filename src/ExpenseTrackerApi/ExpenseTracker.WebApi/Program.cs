using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerApi.Configuration;
using ExpenseTrackerApi.DbContext;
using ExpenseTrackerApi.ExceptionHandler;

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
    });


builder.Services.AddOpenApi();
builder.Services.ConfigureCors();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.ConfigureSwagger();



builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<DbUpdateExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Enable authorization
builder.Services.AddAuthorization();
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog API ");
});


app.UseStaticFiles(); // Enable static files ( html , css , js , images .. )  to be served
app.UseRouting(); // maps incoming requests to route handlers

app.UseCors("CorsPolicy"); // allowing or blocking  requests from different origins ( cross-origin requests )

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
