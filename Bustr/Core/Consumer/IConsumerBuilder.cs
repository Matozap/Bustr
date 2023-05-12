using MassTransit;

namespace Bustr.Core.Consumer;

internal interface IConsumerBuilder
{
    internal void AddConsumersFromConfiguration(IRegistrationContext context, IReceiveEndpointConfigurator configurator, string className);
}