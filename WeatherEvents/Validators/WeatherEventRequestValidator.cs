using FluentValidation;
using WeatherEvents.DTOs;

namespace WeatherEvents.Validators;

public class WeatherEventRequestValidator : AbstractValidator<WeatherEventRequest>
{
    public WeatherEventRequestValidator()
    {
        // StationId validation
        RuleFor(x => x.StationId)
            .NotEmpty().WithMessage("StationId is required.")
            .Length(2, 50).WithMessage("StationId must be between 2 and 50 characters.");

        // Timestamp validation
        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required.")
            .LessThanOrEqualTo(x => DateTime.Now).WithMessage("Timestamp cannot be in the future.");

        // Temperature validation
        RuleFor(x => x.Temperature)
            .NotEmpty().WithMessage("Temperature is required.")
            .InclusiveBetween(-100, 100).WithMessage("Temperature must be between -100°C and 100°C.");

        // Humidity validation
        RuleFor(x => x.Humidity)
            .NotEmpty().WithMessage("Humidity is required.")
            .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0% and 100%.");

        // Pressure validation
        RuleFor(x => x.Pressure)
            .NotEmpty().WithMessage("Pressure is required.")
            .InclusiveBetween(800, 1200).WithMessage("Pressure must be between 800 hPa and 1200 hPa.");

        // WindSpeed validation
        RuleFor(x => x.WindSpeed)
            .NotEmpty().WithMessage("Wind speed is required.")
            .InclusiveBetween(0, 300).WithMessage("Wind speed must be between 0 km/h and 300 km/h.");

        // SequenceNumber validation
        RuleFor(x => x.SequenceNumber)
            .NotEmpty().WithMessage("Sequence number is required.")
            .Length(1, 100).WithMessage("Sequence number must be between 1 and 100 characters.");
    }
}