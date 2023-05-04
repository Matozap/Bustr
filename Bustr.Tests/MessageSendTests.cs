using System.Text.Json;
using Bustr.Bus;
using Bustr.Tests.Events;
using FluentAssertions;
using Xunit;

namespace Bustr.Tests;

public class MessageSendTests
{
    private readonly IEventBus _eventBus;
    
    public MessageSendTests(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    [Fact]
    public async Task TestPublishingSuccess()
    {
        var key = $"randomEvent{DateTime.Now:hh-mm-ss}";
        var randomEvent = new RandomEvent { Key = key, Value = "Success" };
        await Task.Delay(5000);
        
        await _eventBus.PublishAsync(randomEvent);
        await Task.Delay(5000);

        var contents = Environment.GetEnvironmentVariable("RandomEvent") ?? "";
        var obtainedEvent = JsonSerializer.Deserialize<RandomEvent>(contents);

        obtainedEvent.Should().NotBeNull();
        obtainedEvent?.Key.Should().Be(randomEvent.Key);
        obtainedEvent?.Value.Should().Be(randomEvent.Value);
    }
}