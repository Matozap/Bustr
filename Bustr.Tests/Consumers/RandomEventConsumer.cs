using System.Text.Json;
using Bustr.Tests.Events;
using MassTransit;

namespace Bustr.Tests.Consumers;

public class RandomEventConsumer : IConsumer<RandomEvent>
{
    public async Task Consume(ConsumeContext<RandomEvent> context)
    {
        var eventData = context.Message;
        var content = JsonSerializer.Serialize(eventData);
        Environment.SetEnvironmentVariable("RandomEvent", content);
        await Task.CompletedTask;
    }
}