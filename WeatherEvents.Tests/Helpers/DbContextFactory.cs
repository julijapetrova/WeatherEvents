using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WeatherEvents.Data;

namespace WeatherEvents.Tests.Helpers
{
    public static class DbContextFactory
    {
        public static async Task<WeatherReadingDbContext> Create()
        {
            //appsettings.Test.json is not automatically loaded by xUnit
            //factory explicitly creates the configuration
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException(
        "Test database connection string is missing."); ;

            var options = new DbContextOptionsBuilder<WeatherReadingDbContext>()
                 .UseSqlServer(connectionString)
                 .Options;
            var context = new WeatherReadingDbContext(options);

            await context.Database.MigrateAsync();

            return context;
        }
    }
}
