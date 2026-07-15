using Microsoft.EntityFrameworkCore;
using WeatherEvents.Data;
using WeatherEvents.Models;

namespace WeatherEvents.Repositories
{
    public class WeatherRepository : IWeatherRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WeatherRepository> _logger;
        public WeatherRepository(AppDbContext context, ILogger<WeatherRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WeatherEvent> AddReadingAsync(WeatherEvent reading)
        {
            try
            {
                _context.WeatherEvents.Add(reading);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Added weather reading: StationId={StationId}, Id={Id}",
                    reading.StationId,
                    reading.Id);
                return reading;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add weather reading for station {StationId}.", reading.StationId);
                throw new InvalidOperationException("Failed to save the weather reading.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a weather reading.");
                throw;
            }
        }

        public async Task<WeatherEvent?> GetReadingAsync(long id)
        {
            try
            {
                var reading = await _context.WeatherEvents.FindAsync(id);
                if (reading == null)
                {
                    _logger.LogWarning("No weather reading found with ID {Id}.", id);
                }
                return reading;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve weather reading with ID {Id}.", id);
                throw;
            }

        }
    }
}
