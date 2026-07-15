using Newtonsoft.Json.Linq;
using WeatherEvents.Models;
using WeatherEvents.Repositories;
using WeatherEvents.Tests.Helpers;

namespace WeatherEvents.Tests.Repositories
{
    public class WeatherRepositoryTests
    {
        //Method_ShouldExpectedResult_WhenCondition
        [Fact]
        public async Task AddReadingAsync_ShouldPersistReading()
        {
            // Arrange
            await using var context = await DbContextFactory.Create();
            var repository = new WeatherRepository(context);
            var reading = new WeatherEvent
            {

                StationId = "StationId123",
                Timestamp = System.DateTime.Parse("2026-07-15T15:46:56.959Z"),
                Temperature = 100,
                Humidity = 100,
                Pressure = 1200,
                WindSpeed = 300,
                SequenceNumber = "SequenceNumber123"

            };
            // Act
            var weatherEvent = await repository.AddReadingAsync(reading);
            // Assert

            //The returned entity is not null.
            Assert.False(weatherEvent == null);
            //An ID is assigned(if database - generated).
            Assert.False(weatherEvent.Id == null);
            //The row exists in the database.

            //The saved values match what you passed in.
            Assert.Equal(reading.SequenceNumber, weatherEvent.SequenceNumber);
        }
        [Fact]
        public async Task GetReadingAsync_ShouldReturnReading_WhenReadingExists()
        {
            /*
                Arrange:

                Seed a reading into the database.

                Act:

                Call GetReadingAsync(id).

                Assert:

                The returned entity is not null.
                The values are correct.
             */
        }
        [Fact]
        public async Task GetReadingAsync_ShouldReturnNull_WhenReadingDoesNotExist()
        {
            /*
             * Arrange:

Empty database.

Act:

await repository.GetReadingAsync(9999);

Assert:

result.Should().BeNull();
             */
        }
    }
}
