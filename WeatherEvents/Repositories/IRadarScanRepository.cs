using WeatherEvents.Models;
using WeatherEvents.Queues;

public interface IRadarScanRepository
{
    Task<RadarScan> GetByScanIdAsync(string scanId);
    Task<RadarScan> AddScanAsync(RadarScan radarScan);
    Task<List<RadarScan>> GetRecentScansAsync(int hoursBack);
}