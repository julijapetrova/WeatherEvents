# WeatherEvents

Your WeatherEvents project is a .NET 10.0 based REST API for processing weather station data. It allows users to:

Submit weather readings (e.g., temperature, humidity, pressure, wind speed) from weather stations via a POST /weather-readings endpoint.
Retrieve weather readings by ID via a GET /weather-readings/{id} endpoint.
Validate incoming data using FluentValidation to ensure correctness (e.g., temperature range, humidity percentage, wind speed limits).
Log events for debugging and monitoring.
The API is built with:

ASP.NET Core (Minimal API style)
FluentValidation for request validation
Swagger/OpenAPI for API documentation
Logging for tracking events
