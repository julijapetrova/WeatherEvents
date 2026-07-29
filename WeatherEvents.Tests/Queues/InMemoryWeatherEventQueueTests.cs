using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections;
using System.Collections.Concurrent;
using WeatherEvents.Queues;
using WeatherEvents.Tests.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WeatherEvents.Tests.Queues
{
    public class InMemoryWeatherEventQueueTests
    {
        [Fact]
        public void InMemoryWeatherEventQueue_ShouldStartWithEmptyQueue()
        {
            //Arrange:
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();

            //Act:
            var count = weatherEventQueue.Count;

            //Assert:
            count.Should().Be(0);
        }
        [Fact]
        public async Task EnqueueAsync_ShouldIncreaseCountAsync()
        {
            //Arrange:
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();
            WeatherEventWorkItem workItem = new WeatherEventWorkItem();
            //Act:
            await weatherEventQueue.EnqueueAsync(workItem);

            //Assert:
            weatherEventQueue.Count.Should().Be(1);
        }
        [Fact]
        public async Task DequeueAsync_ShouldReturnTheEnqueuedItem()
        {
            //Arrange:
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();
            WeatherEventWorkItem workItem = new WeatherEventWorkItem();
            await weatherEventQueue.EnqueueAsync(workItem);

            //Act:
            var result = await weatherEventQueue.DequeueAsync(CancellationToken.None);

            //Assert:
            result.Should().BeSameAs(workItem);
        }
        [Fact]
        public async Task DequeueAsync_ShouldDecreaseQueueCount()
        {
            //Arrange:
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();
            WeatherEventWorkItem workItem = new WeatherEventWorkItem();
            await weatherEventQueue.EnqueueAsync(workItem);

            //Act:
            await weatherEventQueue.DequeueAsync(CancellationToken.None);

            //Assert:
            weatherEventQueue.Count.Should().Be(0);

        }
        [Fact]
        public async Task DequeueAsync_ShouldReturnItemsInFifoOrder()
        {
            // Arrange
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();

            WeatherEventWorkItem workItem1 = new WeatherEventWorkItem();
            WeatherEventWorkItem workItem2 = new WeatherEventWorkItem();
            WeatherEventWorkItem workItem3 = new WeatherEventWorkItem();

            await weatherEventQueue.EnqueueAsync(workItem1);
            await weatherEventQueue.EnqueueAsync(workItem2);
            await weatherEventQueue.EnqueueAsync(workItem3);

            // Act
            var first = await weatherEventQueue.DequeueAsync(CancellationToken.None);
            var second = await weatherEventQueue.DequeueAsync(CancellationToken.None);
            var third = await weatherEventQueue.DequeueAsync(CancellationToken.None);

            // Assert
            first.Should().BeSameAs(workItem1);
            second.Should().BeSameAs(workItem2);
            third.Should().BeSameAs(workItem3);
        }
        [Fact]
        public async Task EnqueueAsync_ShouldAllowMultipleItemsToBeQueued()
        {
            //Arraange
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();
            //Act
            for (int i = 0; i < 100; i++)
            {
                WeatherEventWorkItem workItem = new WeatherEventWorkItem();
                await weatherEventQueue.EnqueueAsync(workItem);
            }


            //Assert 
            weatherEventQueue.Count.Should().Be(100);

        }
        [Fact]
        public async Task DequeueAsync_WhenQueueIsEmpty_ShouldWaitUntilAnItemIsAvailable()
        {
            // Arrange
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();
            WeatherEventWorkItem workItem = new WeatherEventWorkItem();

            // Act
            ValueTask<WeatherEventWorkItem> dequeueOperation =
                weatherEventQueue.DequeueAsync(CancellationToken.None);

            await weatherEventQueue.EnqueueAsync(workItem);

            WeatherEventWorkItem result = await dequeueOperation;

            // Assert
            result.Should().BeSameAs(workItem);
            weatherEventQueue.Count.Should().Be(0);
        }
        [Fact]
        public async Task EnqueueAsync_WhenCalledByMultipleProducers_ShouldNotLoseItems()
        {
            // Arrange
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();

            var workItems = Enumerable.Range(0, 100)
                .Select(i =>
                {
                    var item = new WeatherEventWorkItem();
                    return item;
                })
                .ToList();

            // Act
            var enqueueTasks = workItems
                .Select(item => weatherEventQueue.EnqueueAsync(item).AsTask())
                .ToList();

            await Task.WhenAll(enqueueTasks);

            var results = new List<WeatherEventWorkItem>();

            for (int i = 0; i < workItems.Count; i++)
            {
                results.Add(await weatherEventQueue.DequeueAsync(CancellationToken.None));
            }

            // Assert
            results.Should().HaveCount(workItems.Count);
            results.Should().BeEquivalentTo(workItems);
            weatherEventQueue.Count.Should().Be(0);
        }
        [Fact]
        public async Task DequeueAsync_WhenMultipleConsumersAreReading_ShouldNotReturnDuplicateItems()
        {
            // Arrange
            IWeatherEventQueue weatherEventQueue = new InMemoryWeatherEventQueue();

            var workItems = Enumerable.Range(0, 100)
                .Select(_ => new WeatherEventWorkItem())
                .ToList();

            foreach (var item in workItems)
            {
                await weatherEventQueue.EnqueueAsync(item);
            }

            var receivedItems = new ConcurrentBag<WeatherEventWorkItem>();

            // Act
            var consumerTasks = Enumerable.Range(0, 10)
                .Select(async _ =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var item = await weatherEventQueue.DequeueAsync(CancellationToken.None);
                        receivedItems.Add(item);
                    }
                })
                .ToList();

            await Task.WhenAll(consumerTasks);

            // Assert
            receivedItems.Should().HaveCount(100);
            receivedItems.Should().BeEquivalentTo(workItems);
            weatherEventQueue.Count.Should().Be(0);
        }
    }
}