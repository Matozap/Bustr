using Bustr.Bus;
using MassTransit;

namespace Bustr.Core.Subscription;

internal interface ISubscriptionBuilder
{
    internal void MapEventToTopic(Type type, string topicName, IInMemoryBusFactoryConfigurator? inMemoryBusFactoryConfigurator,
        IServiceBusBusFactoryConfigurator? serviceBusBusFactoryConfigurator, IRabbitMqBusFactoryConfigurator? rabbitMqBusFactoryConfigurator);

    internal void RegisterSubscriptionDynamically<T>(IRegistrationContext context, List<EventBusOptions.Subscription> subscriptions, IBusFactoryConfigurator<T> busFactoryConfigurator)
        where T : IReceiveEndpointConfigurator;
}