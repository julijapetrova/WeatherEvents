using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherEvents.DTOs;
using WeatherEvents.Models;
using WeatherEvents.Queues;
using WeatherEvents.Repositories;

namespace WeatherEvents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeatherReadingsController : ControllerBase
{
    private readonly IValidator<WeatherEventRequest> _validator;
    private readonly ILogger<WeatherReadingsController> _logger;
    private readonly IWeatherRepository _repository;
    private readonly IWeatherEventQueue _queue;

    public WeatherReadingsController(
        IValidator<WeatherEventRequest> validator,
        IWeatherRepository repository,
        ILogger<WeatherReadingsController> logger,
        IWeatherEventQueue queue
       )
    {
        _validator = validator;
        _repository = repository;
        _logger = logger;
        _queue = queue;

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

        _logger.LogInformation($"Weather reading received from {request.StationId} at {request.Timestamp}");

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
            await _queue.EnqueueAsync(new WeatherEventWorkItem
            {
                WeatherEvent = weatherEvent,
                RetryCount = 0
            });

            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create weather reading.");
            return StatusCode(500, "An error occurred while saving the weather reading.");
        }
    }

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