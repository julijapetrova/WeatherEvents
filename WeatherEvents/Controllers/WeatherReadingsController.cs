using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherEvents.Data;
using WeatherEvents.DTOs;
using WeatherEvents.Models;
using WeatherEvents.Queues;
using WeatherEvents.Repositories;
using WeatherEvents.Services;

namespace WeatherEvents.Controllers;

[ApiController]
[Route("weather-readings")]
public class WeatherReadingsController : ControllerBase
{
    private readonly IValidator<WeatherEventRequest> _validator;
    private readonly ILogger<WeatherReadingsController> _logger;
    private readonly IWeatherRepository _repository;
    private readonly IWeatherEventQueue _queue;

    private readonly IDmiRadarApiClient _client;

    public WeatherReadingsController(
        IValidator<WeatherEventRequest> validator,
        IWeatherRepository repository,
        ILogger<WeatherReadingsController> logger,
        IWeatherEventQueue queue,
        IDmiRadarApiClient client)
    {
        _validator = validator;
        _repository = repository;
        _logger = logger;
        _queue = queue;
        _client = client;

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
    [HttpGet("test-scans")]
    public async Task<IActionResult> TestScans()
    {
        var pastDate = new DateTime(2021, 8, 6, 7, 35, 0, DateTimeKind.Utc);
        var scans = await _client.GetScansAsync(
        pastDate.AddHours(-1),
        pastDate.AddHours(1));


        return Ok(scans.Select(s => new {
            s.Id,
            s.Properties.Datetime,
            DownloadUrl = s.DownloadUrl,
        }));
    }

    [HttpGet("rain-check")]
    public async Task<IActionResult> RainCheck([FromQuery] double lat, [FromQuery] double lon)
    {
        var scan = await _client.GetLatestScanForPointAsync(lat, lon);

        if (scan == null)
            return NotFound("No scan covers this point");

        return Ok(new
        {
            scan.Id,
            scan.Properties.Datetime,
            scan.Geometry?.Bbox,
            DownloadUrl = scan.DownloadUrl
        });
    }
}