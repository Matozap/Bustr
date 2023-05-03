namespace Bustr.Bus;

public interface IEventBus
{
    Task PublishAsync<T>(T message) where T : class;
    Task SendAsync<T>(T message, string destination, bool isTopic = false) where T : class;
}