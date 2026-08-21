using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WeatherEvents.Data;
using WeatherEvents.Models;

namespace WeatherEvents.Repositories
{
    public class RadarScanRepository : IRadarScanRepository
    {
        private readonly WeatherReadingDbContext _context;
        private readonly ILogger<RadarScanRepository> _logger;
        public RadarScanRepository(WeatherReadingDbContext context, ILogger<RadarScanRepository>? logger = null)
        {
            _context = context;
            _logger = logger ?? NullLogger<RadarScanRepository>.Instance; ;
        }
        public async Task<RadarScan> AddScanAsync(RadarScan scan)
        {
            try
            {
                _context.RadarScans.Add(scan);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Added scan: StationId={StationId}, Id={Id}",
                    scan.StationId,
                    scan.Id);
                return scan;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a scan.");
                throw;
            }
        }

        public async Task<RadarScan> GetByScanIdAsync(string scanId)
        {
            try
            {
                var scan = await _context.RadarScans.FirstOrDefaultAsync(s => s.ScanId == scanId);
                if (scan == null)
                {
                    _logger.LogDebug("No  scan found with ID {Id}.", scanId);
                }
                return scan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve scan with ID {Id}.", scanId);
                throw;
            }
        }

        public async Task<List<RadarScan>> GetRecentScansAsync(int hoursBack)
        {
            return await _context.RadarScans
                .Where(s => s.ScanTime >= DateTime.UtcNow.AddHours(-hoursBack))
                .OrderByDescending(s => s.ScanTime)
                .ToListAsync();
        }
    }
}