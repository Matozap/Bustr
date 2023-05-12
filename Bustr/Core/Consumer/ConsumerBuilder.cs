using Bustr.Bus;
using MassTransit;

namespace Bustr.Core.Consumer;

internal class ConsumerBuilder : IConsumerBuilder
{
    private readonly EventBusOptions _eventBusOptions;
    

    public ConsumerBuilder(EventBusOptions eventBusOptions)
    {
        _eventBusOptions = eventBusOptions;
    }
    
    public void AddConsumersFromConfiguration(IRegistrationContext context, IReceiveEndpointConfigurator configurator, string className)
    {
        var interfaceType = typeof(IConsumer);
        var consumerTypes =
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false })
                .ToList();
        
        foreach (var consumerType in consumerTypes.Where(consumerType => consumerType.Name == className))
        {
            configurator.ConfigureConsumer(context, consumerType);
        }

        if (_eventBusOptions.ImmediateRetry)
        {
            configurator.UseMessageRetry(r => r.Immediate(_eventBusOptions.RetryCount));
        }
        else
        {
            configurator.UseDelayedRedelivery(r => r.Intervals(_eventBusOptions.RetryIntervals));
        }

        if (!_eventBusOptions.DeadLetterQueueEnabled) return;
        
        configurator.ConfigureDeadLetterQueueDeadLetterTransport();
        configurator.ConfigureDeadLetterQueueErrorTransport();
        configurator.PublishFaults = false;
    }
}
