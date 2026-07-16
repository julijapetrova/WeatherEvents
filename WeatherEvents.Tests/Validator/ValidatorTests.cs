using WeatherEvents.DTOs;
using WeatherEvents.Validators;
using FluentAssertions;
using WeatherEvents.Tests.Helpers;

namespace WeatherEvents.Tests.Validator
{
    public class ValidatorTests
    {
        [Fact]
        public void Validator_Test()
        {
            //Arrange
            var validator = new WeatherEventRequestValidator();
            var request = KnownGood.Request();
            //Act
            var result = validator.Validate(request);
            //Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
