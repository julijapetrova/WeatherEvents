# WeatherEvents

## What the Project Does
"This is a weather data backend system demonstrating .NET API development, database modeling, and background processing."
## Context Diagram
```plaintext
+---------------------+       +---------------------+       +---------------------+
|   Weather Station   |       |  WeatherEvents API  |       |       Client        |
|  (External Source)  |------>|                     |------>|                     |
+---------------------+       +---------------------+       +---------------------+
```
- Submit weather readings (e.g., temperature, humidity, pressure, wind speed) from weather stations via a `POST /weather-readings` endpoint.
- Retrieve weather readings by ID via a `GET /weather-readings/{id}` endpoint.
- Validate incoming data using **FluentValidation** to ensure correctness (e.g., temperature range, humidity percentage, wind speed limits).
- Log events for debugging and monitoring.

The API is built with:
- **ASP.NET Core** (Minimal API style)
- **FluentValidation** for request validation
- **Swagger/OpenAPI** for API documentation
- **Logging** for tracking events

## Running with Docker

`bash docker-compose up --build`

Then visit http://localhost:5000/swagger
