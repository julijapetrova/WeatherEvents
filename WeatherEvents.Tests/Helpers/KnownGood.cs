
using WeatherEvents.DTOs;
using WeatherEvents.Models;

namespace WeatherEvents.Tests.Helpers
{
    public class KnownGood
    {
        public static WeatherEventRequest Request() => new()
        {
            StationId = "STATION_01",
            Timestamp = DateTime.Now.AddMinutes(-5),
            Temperature = 22.5M,
            Humidity = 65,
            Pressure = 1013,
            WindSpeed = 12,
            SequenceNumber = "SEQ-001"
        };
        public static WeatherEvent Reading() => new()
        {
            StationId = "StationId123",
            Timestamp = System.DateTime.Parse("2026-07-15T15:46:56.959Z"),
            Temperature = 22.5M,
            Humidity = 65,
            Pressure = 1013,
            WindSpeed = 12,
            SequenceNumber = "SequenceNumber123"

        };
    }
}
