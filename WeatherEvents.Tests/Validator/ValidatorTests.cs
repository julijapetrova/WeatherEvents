using FluentAssertions;
using WeatherEvents.DTOs;
using WeatherEvents.Tests.Helpers;
using WeatherEvents.Validators;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WeatherEvents.Tests.Validator
{
    public class ValidatorTests
    {
        [Fact]
        public void Validate_ShouldPass_WhenRequestIsValid()
        {
            //Arrange
            var validator = new WeatherEventRequestValidator();
            var request = KnownGood.Request();
            //Act
            var result = validator.Validate(request);
            //Assert
            result.IsValid.Should().BeTrue();
        }
        [Fact]
        public void Validate_ShouldFail_WhenRequiredFieldMissing()
        {
            //Arrange
            var validator = new WeatherEventRequestValidator();
            var request = KnownGood.Request();
            request.StationId = "";

            //Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "StationId");
        }
        [Fact]
        public void Validate_ShouldFail_WhenTemperatureTooLow()
        {
            //Arrange
            var validator = new WeatherEventRequestValidator();
            var request = KnownGood.Request();
            request.Temperature = -200;

            //Act
            var result = validator.Validate(request);

            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Temperature");
        }
        [Fact]
        public void Validate_ShouldFail_WhenTemperatureTooHigh()
        {
            //Arrange
            var validator = new WeatherEventRequestValidator();
            var request = KnownGood.Request();
            request.Temperature = 200;

            //Act
            var result = validator.Validate(request);

            //Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Temperature");
        }
        [Fact]
        public void Validate_ShouldReportMultipleErrors_WhenMultipleFieldsInvalid()
        {
            //Arrange
            var validator = new WeatherEventRequestValidator();
            var request = KnownGood.Request();
            request.Temperature = 200;
            request.StationId = "";
            request.WindSpeed = -1;

            //Act
            var result = validator.Validate(request);

            //Assert
            result.IsValid.Should().BeFalse();
            //Why 4? Errors: 1: NotEmpty fails 2: Length(2, 50) fails 3: Temperature out of range 4: WindSpeed out of range
            result.Errors.Should().HaveCount(4);
            result.Errors.Select(e => e.PropertyName)
                .Should()
                .BeEquivalentTo("Temperature", "StationId", "StationId", "WindSpeed");
        }
    }
}
