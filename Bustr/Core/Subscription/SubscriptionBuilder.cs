using System.Reflection;
using Bustr.Bus;
using Bustr.Core.Consumer;
using MassTransit;

namespace Bustr.Core.Subscription;

internal class SubscriptionBuilder
{
    private readonly EventBusOptions _eventBusOptions;

    public SubscriptionBuilder(EventBusOptions eventBusOptions)
    {
        _eventBusOptions = eventBusOptions;
    }

    private void SetMessageTopologyInMemory<T>(IInMemoryBusFactoryConfigurator busFactoryConfigurator, string topicName) where T : class
    {
        busFactoryConfigurator.Message<T>(e => e.SetEntityName(topicName));
    }
    
    private void SetMessageTopologyServiceBus<T>(IServiceBusBusFactoryConfigurator busFactoryConfigurator, string topicName) where T : class
    {
        busFactoryConfigurator.Message<T>(e => e.SetEntityName(topicName));
    }
    
    private void SetMessageTopologyRabbitMq<T>(IRabbitMqBusFactoryConfigurator busFactoryConfigurator, string topicName) where T : class
    {
        busFactoryConfigurator.Message<T>(e => e.SetEntityName(topicName));
    }
    
    private void SetMessageTopologyAmazonSqs<T>(IAmazonSqsBusFactoryConfigurator busFactoryConfigurator, string topicName) where T : class
    {
        busFactoryConfigurator.Message<T>(e => e.SetEntityName(topicName));
    }
    
    internal void MapEventToTopic(Type type, string topicName, IInMemoryBusFactoryConfigurator? inMemoryBusFactoryConfigurator,  
        IServiceBusBusFactoryConfigurator? serviceBusBusFactoryConfigurator, IRabbitMqBusFactoryConfigurator? rabbitMqBusFactoryConfigurator,
        IAmazonSqsBusFactoryConfigurator? amazonSqsBusFactoryConfigurator)
    {
        var methodInfo = serviceBusBusFactoryConfigurator != null ? typeof(SubscriptionBuilder).GetMethod(nameof(SetMessageTopologyServiceBus), BindingFlags.NonPublic | BindingFlags.Instance)
            : rabbitMqBusFactoryConfigurator != null ? typeof(SubscriptionBuilder).GetMethod(nameof(SetMessageTopologyRabbitMq), BindingFlags.NonPublic | BindingFlags.Instance)
            : amazonSqsBusFactoryConfigurator != null ? typeof(SubscriptionBuilder).GetMethod(nameof(SetMessageTopologyAmazonSqs), BindingFlags.NonPublic | BindingFlags.Instance)
            : typeof(SubscriptionBuilder).GetMethod(nameof(SetMessageTopologyInMemory), BindingFlags.NonPublic | BindingFlags.Instance);
        
        if(methodInfo == null) return;
        
        object? cfg = serviceBusBusFactoryConfigurator != null ? serviceBusBusFactoryConfigurator
            : rabbitMqBusFactoryConfigurator != null ? rabbitMqBusFactoryConfigurator
            : amazonSqsBusFactoryConfigurator != null ? amazonSqsBusFactoryConfigurator
            : inMemoryBusFactoryConfigurator;
        
        var generic = methodInfo.MakeGenericMethod(type);
        generic.Invoke(this, new[]{ cfg, topicName });
    }

    internal void RegisterSubscriptionDynamically<T>(IRegistrationContext context, List<EventBusOptions.Subscription> subscriptions, IBusFactoryConfigurator<T> busFactoryConfigurator) where T : IReceiveEndpointConfigurator
    {
        var consumerBuilder = new ConsumerBuilder(_eventBusOptions);
        foreach (var subscription in subscriptions.Where(subscription => !string.IsNullOrWhiteSpace(subscription.SubscriptionName)))
        {
            switch (busFactoryConfigurator)
            {
                case IServiceBusBusFactoryConfigurator azureBusConfigurator:
                    azureBusConfigurator.SubscriptionEndpoint(subscription.SubscriptionName,subscription.TopicPath, configurator =>
                    {
                        consumerBuilder.AddConsumersFromConfiguration(context, configurator, subscription.ConsumerType.Name);
                    });
                    break;
                case IRabbitMqBusFactoryConfigurator rabbitBusConfigurator:
                    rabbitBusConfigurator.ReceiveEndpoint(subscription.SubscriptionName, configurator =>
                    {
                        configurator.ExchangeType = "direct";
                        consumerBuilder.AddConsumersFromConfiguration(context, configurator, subscription.ConsumerType.Name);
                    });
                    break;
                case IAmazonSqsBusFactoryConfigurator amazonSqsBusFactoryConfigurator:
                    amazonSqsBusFactoryConfigurator.ReceiveEndpoint(subscription.SubscriptionName, configurator =>
                    {
                        consumerBuilder.AddConsumersFromConfiguration(context, configurator, subscription.ConsumerType.Name);
                    });
                    break;
            }
        }
    }
}