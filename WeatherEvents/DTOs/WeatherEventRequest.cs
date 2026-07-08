using System.ComponentModel.DataAnnotations;

namespace WeatherEvents.DTOs;

public class WeatherEventRequest
{
    [Required]
    public string StationId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    [Range(-100, 100)]
    public decimal Temperature { get; set; }

    [Range(0, 100)]
    public decimal Humidity { get; set; }

    [Range(800, 1200)]
    public decimal Pressure { get; set; }

    [Range(0, 200)]
    public decimal WindSpeed { get; set; }

    public long SequenceNumber { get; set; }
}