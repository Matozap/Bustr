using Bustr.Bus;
using MassTransit;

namespace Bustr.Core.Subscription;

public interface ISubscriptionBuilder
{
    void MapEventToTopic(Type type, string topicName, IInMemoryBusFactoryConfigurator? inMemoryBusFactoryConfigurator,
        IServiceBusBusFactoryConfigurator? serviceBusBusFactoryConfigurator, IRabbitMqBusFactoryConfigurator? rabbitMqBusFactoryConfigurator);

    void RegisterSubscriptionDynamically<T>(IRegistrationContext context, List<EventBusOptions.Subscription> subscriptions, IBusFactoryConfigurator<T> busFactoryConfigurator)
        where T : IReceiveEndpointConfigurator;
}