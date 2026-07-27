using System;
using System.Collections.Generic;
using System.Text;
using WeatherEvents.Queues;
using WeatherEvents.Tests.Helpers;

namespace WeatherEvents.Tests.Queues
{
    public  class InMemoryWeatherEventQueueTests
    {
        [Fact]
        public async Task EnqueueAsync_ShouldIncreaseCount()
        {
            // Arrange
            IWeatherEventQueue queue = new InMemoryWeatherEventQueue();

            var workItem = new WeatherEventWorkItem
            {
                WeatherEvent = KnownGood.Reading()
            };

            // Act
            await queue.EnqueueAsync(workItem);

            // Assert
            Assert.Equal(1, queue.Count);
        }
        [Fact]
        public async Task DequeueAsync_ShouldDecreaseCount()
        {
            // Arrange
            IWeatherEventQueue queue = new InMemoryWeatherEventQueue();

            var workItem = new WeatherEventWorkItem
            {
                WeatherEvent = KnownGood.Reading()
            };

            await queue.EnqueueAsync(workItem);

            // Act
            var result = await queue.DequeueAsync(CancellationToken.None);

            // Assert
            Assert.Equal(0, queue.Count);
            Assert.Equal(workItem, result);
        }
        [Fact]
        public async Task DequeueAsync_ShouldReturnItemsInOrder()
        {
            IWeatherEventQueue queue = new InMemoryWeatherEventQueue();

            var first = new WeatherEventWorkItem
            {
                WeatherEvent = KnownGood.Reading()
            };

            first.WeatherEvent.SequenceNumber = "1";

            var second = new WeatherEventWorkItem
            {
                WeatherEvent = KnownGood.Reading()
            };

            second.WeatherEvent.SequenceNumber = "2";

            await queue.EnqueueAsync(first);
            await queue.EnqueueAsync(second);

            var result1 = await queue.DequeueAsync(CancellationToken.None);
            var result2 = await queue.DequeueAsync(CancellationToken.None);

            Assert.Equal("1", result1.WeatherEvent.SequenceNumber);
            Assert.Equal("2", result2.WeatherEvent.SequenceNumber);
        }
        [Fact]
        public async Task DeadLetterQueue_ShouldStoreFailedItems()
        {
            IDeadLetterQueue queue = new InMemoryDeadLetterQueue();

            var workItem = new WeatherEventWorkItem
            {
                WeatherEvent = KnownGood.Reading()
            };

            await queue.EnqueueAsync(workItem);

            Assert.Equal(1, queue.Count);

            var result = await queue.DequeueAsync(CancellationToken.None);

            Assert.Equal(workItem, result);
            Assert.Equal(0, queue.Count);
        }
    }
}
