using System.ComponentModel.DataAnnotations;

namespace WeatherEvents.DTOs;

public class WeatherEventRequest
{
    [Required(ErrorMessage = "StationId is required")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "StationId must be between 2 and 50 characters")]
    public string StationId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Timestamp is required")]
    public DateTime Timestamp { get; set; }

    [Range(-100, 100,
        ErrorMessage = "Temperature must be between -100°C and 100°C")]
    public decimal Temperature { get; set; }

    [Range(0, 100,
        ErrorMessage = "Humidity must be between 0% and 100%")]
    public decimal Humidity { get; set; }

    [Range(800, 1200,
        ErrorMessage = "Pressure must be between 800 hPa and 1200 hPa")]
    public decimal Pressure { get; set; }

    [Range(0, 300,
        ErrorMessage = "Wind speed must be between 0 km/h and 300 km/h")]
    public decimal WindSpeed { get; set; }

    [Range(1, long.MaxValue,
        ErrorMessage = "Sequence number must be a positive integer")]
    public long SequenceNumber { get; set; }
}