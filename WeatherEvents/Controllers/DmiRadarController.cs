using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WeatherEvents.DTOs;
using WeatherEvents.Queues;
using WeatherEvents.Repositories;
using WeatherEvents.Services;

namespace WeatherEvents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DmiRadarController : ControllerBase
    {
        private readonly ILogger<WeatherReadingsController> _logger;
        private readonly IWeatherEventQueue _queue;
        private readonly IDmiRadarApiClient _client;
        public DmiRadarController(
        ILogger<WeatherReadingsController> logger,
        IWeatherEventQueue queue,
        IDmiRadarApiClient client)
        {
            _logger = logger;
            _queue = queue;
            _client = client;
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
                s.DownloadUrl,               
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
}
