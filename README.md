# WeatherEvents

## What the Project Does

A weather data backend system demonstrating .NET API development, database modeling, and background processing.

## Architecture

+---------------------+       +---------------------+       +---------------------+
|                     |       |                     |       |                     |
|  Weather Station    |------>|  WeatherEvents API  |------>|       Client        |
|  (External Source)  |       |                     |       |                     |
|                     |       |                     |       |                     |
+---------------------+       +---------------------+       +---------------------+

Submit weather readings (e.g., temperature, humidity, pressure, wind speed) from weather stations via a `POST /weather-readings` endpoint.

Retrieve weather readings by ID via a `GET /weather-readings/{id}` endpoint.

Validate incoming data using FluentValidation to ensure correctness (e.g., temperature range, humidity percentage, wind speed limits).

Log events for debugging and monitoring.

### Technologies Used

- ASP.NET Core (Minimal API style)
- FluentValidation for request validation
- Swagger/OpenAPI for API documentation
- EF Core with SQL Server
- Docker Compose for local development

## Features

| Feature | Status |
|---------|--------|
| REST API endpoints (POST/GET weather readings) | ✅ Implemented |
| EF Core database with migrations | ✅ Implemented |
| FluentValidation for request validation | ✅ Implemented |
| Docker Compose local development | ✅ Implemented |
| Background worker for scheduled data collection | 🔄 In Progress |
| DMI Radar API integration | 🔄 In Progress |
| Alert threshold logic | ⏳ Planned |

## Running with Docker
bash docker-compose up --build

Then visit `http://localhost:5000/swagger` for API documentation.
