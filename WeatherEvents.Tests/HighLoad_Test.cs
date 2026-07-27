using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using WeatherEvents.Tests.Helpers;
using WeatherEvents.Validators;
using Xunit;
using Xunit.Abstractions;

namespace WeatherEvents.Tests
{
    public class loadtest
    {
        private readonly ITestOutputHelper _output;

        public loadtest(ITestOutputHelper output)
        {
            _output = output;
        }
        //"What happens if x clients all POST at the same time?"
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public async Task PostWeatherReadings_WithConcurrentRequests_ShouldSucceed(int requestCount)
        {
            //Arrange
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7259")
            };


            //Act
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < requestCount; i++)
            {
                var weatherRequest = KnownGood.Request();
                weatherRequest.SequenceNumber = $"SEQ-{i}";
                var json = JsonSerializer.Serialize(weatherRequest);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");
                tasks.Add(client.PostAsync("/weather-readings", content));
            }
            var stopwatch = Stopwatch.StartNew();

            var responses = await Task.WhenAll(tasks);

            stopwatch.Stop();

            var successCount = responses.Count(r => r.IsSuccessStatusCode);

            _output.WriteLine(
                $"Requests: {requestCount}, " +
                $"Success: {successCount}, " +
                $"Time: {stopwatch.ElapsedMilliseconds}ms");
            //Assert
            Assert.All(responses, response => Assert.True(response.IsSuccessStatusCode));

        }
    }
}