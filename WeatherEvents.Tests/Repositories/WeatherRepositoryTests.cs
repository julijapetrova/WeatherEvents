using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
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
            var reading = KnownGood.Reading();

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
            var reading = KnownGood.Reading();

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
            //Arrange:
            await using var context = await DbContextFactory.Create();
            var repository = new WeatherRepository(context);
            var reading = KnownGood.Reading();
            reading.Id = 0;
            //Act:
            var result = await repository.GetReadingAsync(reading.Id);

            //Assert:
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetReadingAsync_ShouldReturnNull_WhenDatabaseIsEmpty()
        {
            //Arrange:
            await using var context = await DbContextFactory.Create();
            var repository = new WeatherRepository(context);

            //Act:
            var result = await repository.GetReadingAsync(1);

            //Assert:
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddReadingAsync_ShouldReturnSavedEntityWithId()
        {
            //Arrange:
            await using var context = await DbContextFactory.Create();
            var repository = new WeatherRepository(context);
            var reading = KnownGood.Reading();

            //Act:
            var result = await repository.AddReadingAsync(reading);

            //Assert:
            result.StationId.Should().Be(reading.StationId);
            result.Id.Should().BeGreaterThan(0);

        }
        [Fact]
        public async Task AddReadingAsync_ShouldThrowInvalidOperationException_WhenDbUpdateFails()
        {
            // Arrange
            await using var context = await DbContextFactory.Create();
            var repository = new WeatherRepository(context);

            var reading = KnownGood.Reading();
            // StationId exceeds MaxLength(50) defined in DbContext
            reading.StationId = new string('A', 100);

            // Act & Assert
            await FluentActions.Awaiting(() => repository.AddReadingAsync(reading))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to save the weather reading.");
        }

    }
}



