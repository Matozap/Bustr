using System.Text.Json;
using Bustr.Tests.Events;
using MassTransit;

namespace Bustr.Tests.Consumers;

public class RandomEventConsumer : IConsumer<RandomEvent>
{
    public async Task Consume(ConsumeContext<RandomEvent> context)
    {
        var eventData = context.Message;
        var fileContent = JsonSerializer.Serialize(eventData);
        var path = eventData.Key;
        await File.WriteAllTextAsync(path, fileContent);
    }
}