using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherEvents.Data;
using WeatherEvents.DTOs;
using WeatherEvents.Models;
using WeatherEvents.Repositories;

namespace WeatherEvents.Controllers;

[ApiController]
[Route("weather-readings")]
public class WeatherReadingsController : ControllerBase
{
    private readonly IValidator<WeatherEventRequest> _validator;
    private readonly WeatherReadingDbContext _context;
    private readonly ILogger<WeatherReadingsController> _logger;
    private readonly IWeatherRepository _repository;

    public WeatherReadingsController(
        IValidator<WeatherEventRequest> validator,
        WeatherReadingDbContext context,
        IWeatherRepository repository,
        ILogger<WeatherReadingsController> logger)
    {
        _validator = validator;
        _context = context;
        _repository = repository;
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
        try
        {
            // Add to the repository
            var createdEvent = await _repository.AddReadingAsync(weatherEvent);

            // Log the event
            _logger.LogInformation(
                "Weather reading saved: StationId={StationId}, SequenceNumber={SequenceNumber}, Id={Id}",
                createdEvent.StationId,
                createdEvent.SequenceNumber,
                createdEvent.Id);

            // Return 201 (Created) with the created resource
            return CreatedAtAction(
                nameof(GetWeatherReading),
                new { id = createdEvent.Id },
                new
                {
                    createdEvent.Id,
                    createdEvent.StationId,
                    createdEvent.Timestamp,
                    createdEvent.Temperature,
                    createdEvent.Humidity,
                    createdEvent.Pressure,
                    createdEvent.WindSpeed,
                    createdEvent.SequenceNumber
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create weather reading.");
            return StatusCode(500, "An error occurred while saving the weather reading.");
        }
    }

    // Optional: Add a GET endpoint to retrieve a weather reading by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWeatherReading(long id)
    {
        try
        {
            var reading = await _repository.GetReadingAsync(id);
            if (reading == null)
            {
                return NotFound();
            }
            return Ok(reading);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve weather reading with ID {Id}.", id);
            return StatusCode(500, "An error occurred while retrieving the weather reading.");
        }
    }
}