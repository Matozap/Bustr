using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bustr.Bus;

public sealed class EventBus : IEventBus
{
    private readonly EventBusOptions _options;
    private readonly ILogger<EventBus> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IBus _bus;

    public EventBus(EventBusOptions options, ILogger<EventBus> logger, IPublishEndpoint publishEndpoint, IBus bus)
    {
        _options = options;
        _options = options;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _bus = bus;
    }

    public async Task Publish<T>(T message) where T : class
    {
        try
        {
            if (_options.Disabled)
                return;

            await _publishEndpoint.Publish(message);
            _logger.LogInformation("Publishing changes to {Destination}", message.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing changes");
        }
    }

    public async Task Send<T>(T message, string destination, bool isTopic = false) where T : class
    {
        try
        {
            if (!string.IsNullOrEmpty(destination))
            {
                var uri = isTopic ? new Uri($"topic:{destination}") : new Uri($"queue:{destination}");
                var endpoint = await _bus.GetSendEndpoint(uri);
                await endpoint.Send(message);
                _logger.LogInformation("Message sent to {Destination}", destination);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing changes");
        }
    }
}