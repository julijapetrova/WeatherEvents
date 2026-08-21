using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WeatherEvents.Data;
using WeatherEvents.Queues;
using WeatherEvents.Repositories;
using WeatherEvents.Services;
using WeatherEvents.Validators;
using WeatherEvents.Workers;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// MVC / API
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<WeatherReadingDbContext>(options =>
    options.UseSqlServer(connectionString));

// Queues
builder.Services.AddSingleton<
    IWeatherEventQueue,
    InMemoryWeatherEventQueue>();

builder.Services.AddSingleton<
    IDmiRadarEventQueue,
    InMemoryDmiRadarEventQueue>();

builder.Services.AddSingleton<
    IDeadLetterQueue<RadarScanWorkItem>,
    InMemoryDeadLetterQueue<RadarScanWorkItem>>();

builder.Services.AddSingleton<
    IDeadLetterQueue<WeatherEventWorkItem>,
    InMemoryDeadLetterQueue<WeatherEventWorkItem>>();

// Workers
builder.Services.AddHostedService<WeatherEventWorker>();
builder.Services.AddHostedService<DmiRadarProcessor>();
builder.Services.AddHostedService<DmiRadarPoller>();

// Repositories
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IRadarScanRepository, RadarScanRepository>();

// DMI Radar API
builder.Services.AddHttpClient<IDmiRadarApiClient, DmiRadarApiClient>(client =>
{
    var baseUrl = builder.Configuration["DmiRadarApi:BaseUrl"]
        ?? throw new InvalidOperationException(
            "DmiRadarApi:BaseUrl is not configured.");

    var timeoutSeconds = builder.Configuration.GetValue<int>(
        "DmiRadarApi:TimeoutSeconds",
        60);

    if (timeoutSeconds <= 0)
    {
        throw new InvalidOperationException(
            "DmiRadarApi:TimeoutSeconds must be greater than zero.");
    }

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<
    WeatherEventRequestValidator>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Development database migration
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<WeatherReadingDbContext>();

        logger.LogInformation("Attempting to migrate database...");

        await db.Database.MigrateAsync();

        logger.LogInformation(
            "Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(
            ex,
            "Database migration failed.");

        throw;
    }
}

logger.LogInformation(
    "WeatherEvents API started successfully at {Time}",
    DateTime.UtcNow);

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();