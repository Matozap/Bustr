using MassTransit;

namespace Bustr.Core.Consumer;

public interface IConsumerBuilder
{
    void AddConsumersFromConfiguration(IRegistrationContext context, IReceiveEndpointConfigurator configurator, string className);
}