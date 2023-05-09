namespace Bustr.Bus;

public interface IEventBus
{
    /// <summary>
    /// Publish an object using the topic registered for it in the startup/program service registration
    /// </summary>
    /// <param name="message">The object to be sent as a message</param>
    /// <typeparam name="T">T is a class</typeparam>
    /// <returns>Task</returns>
    Task PublishAsync<T>(T message) where T : class;
    /// <summary>
    /// Sends an object after being serialized to the destination
    /// </summary>
    /// <param name="message">The object to be sent as a message</param>
    /// <param name="destination">Destination of the message</param>
    /// <param name="isTopic">Sets if the destination is a topic or a queue</param>
    /// <typeparam name="T">T is a class</typeparam>
    /// <returns>Task</returns>
    Task SendAsync<T>(T message, string destination, bool isTopic = false) where T : class;
}