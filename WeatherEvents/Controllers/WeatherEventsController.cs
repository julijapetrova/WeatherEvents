using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherEvents.DTOs;

namespace WeatherEvents.Controllers;

[ApiController]
[Route("weather-readings")]
public class WeatherReadingsController : ControllerBase
{
    private readonly IValidator<WeatherEventRequest> _validator;
    private readonly ILogger<WeatherReadingsController> _logger;

    public WeatherReadingsController(
        IValidator<WeatherEventRequest> validator,
        ILogger<WeatherReadingsController> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWeatherReading([FromBody] WeatherEventRequest request)
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