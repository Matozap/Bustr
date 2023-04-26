namespace Bustr.Bus;

public interface IEventBus
{
    Task Publish<T>(T message) where T : class;
    Task Send<T>(T message, string destination, bool isTopic = false) where T : class;
}