using WeatherEvents.Models;

namespace WeatherEvents.Repositories
{
    public interface IWeatherRepository
    {
        Task<WeatherEvent> AddReadingAsync(WeatherEvent reading);
        Task<WeatherEvent?> GetReadingAsync(long id);
    }
}
