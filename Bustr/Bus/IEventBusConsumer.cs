using MassTransit;

namespace Bustr.Bus;

public interface IEventBusConsumer<in T>: IConsumer<T> where T : class
{
}