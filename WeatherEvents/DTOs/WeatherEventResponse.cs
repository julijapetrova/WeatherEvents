namespace WeatherEvents.DTOs;

public class WeatherEventResponse
{
    public string StationId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public decimal Temperature { get; set; }

    public decimal Humidity { get; set; }

    public decimal Pressure { get; set; }

    public decimal WindSpeed { get; set; }

    public long SequenceNumber { get; set; }
}