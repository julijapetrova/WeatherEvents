using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherEvents.Data;
using WeatherEvents.DTOs;
using WeatherEvents.Models;

namespace WeatherEvents.Controllers;

[ApiController]
[Route("weather-readings")]
public class WeatherReadingsController : ControllerBase
{
    private readonly IValidator<WeatherEventRequest> _validator;
    private readonly AppDbContext _context;

    private readonly ILogger<WeatherReadingsController> _logger;

    public WeatherReadingsController(
        IValidator<WeatherEventRequest> validator, AppDbContext context,
        ILogger<WeatherReadingsController> logger)
    {
        _validator = validator;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> WeatherReading([FromBody] WeatherEventRequest request)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            // Return 400 with validation errors
            return BadRequest(validationResult.Errors);
        }

        // If validation passes, process the request (e.g., save to database)
        _logger.LogInformation($"Weather reading received from {request.StationId} at {request.Timestamp}");

        // Map the DTO to the WeatherEvent model
        var weatherEvent = new WeatherEvent
        {
            StationId = request.StationId,
            Timestamp = request.Timestamp,
            Temperature = request.Temperature,
            Humidity = request.Humidity,
            Pressure = request.Pressure,
            WindSpeed = request.WindSpeed,
            SequenceNumber = request.SequenceNumber
        };

        // Add to the DbContext and save
        _context.WeatherEvents.Add(weatherEvent);
        await _context.SaveChangesAsync();
        
        // Log the event
        _logger.LogInformation(
            "Weather reading saved: StationId={StationId}, SequenceNumber={SequenceNumber}, Id={Id}",
            weatherEvent.StationId,
            weatherEvent.SequenceNumber,
            weatherEvent.Id);

        // Return 201 (Created) with the created resource
        return CreatedAtAction(
            nameof(GetWeatherReading),
            new { id = request.SequenceNumber },
            request);
    }

    // Optional: Add a GET endpoint to retrieve a weather reading by ID
    [HttpGet("{id}")]
    public IActionResult GetWeatherReading(long id)
    {
        // In a real app, fetch the weather reading from a repository
        return Ok(new { Id = id, Message = "Weather reading retrieved successfully." });
    }
}