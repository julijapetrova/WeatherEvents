using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WeatherEvents.Data;
using WeatherEvents.Repositories;
using WeatherEvents.Validators;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext to the service container
builder.Services.AddDbContext<WeatherReadingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the repository
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

// Add FluentValidation to the services
builder.Services.AddValidatorsFromAssemblyContaining<WeatherEventRequestValidator>();
builder.Configuration.AddEnvironmentVariables();
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation(
    "WeatherEvents API started successfully at {Time}",
    DateTime.UtcNow);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UsePathBase("/swagger");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var logger = context.RequestServices
            .GetRequiredService<ILogger<Program>>();

        var exceptionHandler =
            context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionHandler?.Error != null)
        {
            logger.LogError(
                exceptionHandler.Error,
                "Unhandled exception occurred while processing request");
        }

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(
            "An unexpected error occurred.");
    });
});