using FluentAssertions;
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
                Temperature = 22.5M,
                Humidity = 65,
                Pressure = 1013,
                WindSpeed = 12,
                SequenceNumber = "SequenceNumber123"

            };
            // Act
            var result = await repository.AddReadingAsync(reading);

            var savedReading =
            await context.WeatherEvents.FindAsync(result.Id);
            // Assert

            //The returned entity is not null.
            result.Should().NotBeNull();
            //An ID is assigned(if database - generated).
            result.Id.Should().BeGreaterThan(0);
            //The row exists in the database.
            savedReading.Should().NotBeNull();
            //The saved values match what you passed in.
            savedReading!.SequenceNumber.Should()
            .Be(reading.SequenceNumber);
        }
        [Fact]
        public async Task GetReadingAsync_ShouldReturnReading_WhenReadingExists()
        {            // Arrange

            await using var context = await DbContextFactory.Create();
            var repository = new WeatherRepository(context);
            var reading = new WeatherEvent
            {
                StationId = "StationId123",
                Timestamp = System.DateTime.Parse("2026-07-15T15:46:56.959Z"),
                Temperature = 22.5M,
                Humidity = 65,
                Pressure = 1013,
                WindSpeed = 12,
                SequenceNumber = "SequenceNumber123"
            };
            context.WeatherEvents.Add(reading);
            await context.SaveChangesAsync();
            // Act
            var result = await repository.GetReadingAsync(reading.Id);

            // Assert
            //The returned entity is not null.
            result.Should().NotBeNull();
            //The values are correct.
            result.StationId.Should().Be(reading.StationId);
            result.SequenceNumber.Should().Be(reading.SequenceNumber);
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
