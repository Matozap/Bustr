using System.Reflection;

namespace Bustr.Bus;

public class EventBusOptions
{
    public record Subscription(string TopicPath, Type ConsumerType, string SubscriptionName);
    public string? ConnectionString { get; private set; }
    public BusType BusType { get; private set; }
    public bool Disabled { get; private set; }
    public Dictionary<Type, string> TopicMappings { get; } = new();
    public bool ImmediateRetry { get; private set; } = true;
    public int RetryCount { get; private set; } = 2;
    public bool DeadLetterQueueEnabled { get; private set; }
    public TimeSpan[]? RetryIntervals { get; private set; }
    public List<Subscription> Subscriptions { get; } = new();
    public List<Assembly> Assemblies { get; } = new();

    public EventBusOptions()
    {
    }
    
    public EventBusOptions Configure(BusType busType, string? connectionString = null)
    {
        BusType = busType;
        ConnectionString = connectionString;
        return this;
    }
    
    public EventBusOptions DisableBus(bool disable)
    {
        Disabled = disable;
        return this;
    }
    
    public EventBusOptions UseDeadLetterQueue(bool useIt)
    {
        DeadLetterQueueEnabled = useIt;
        return this;
    }
    
    public EventBusOptions MapTopic(string topicPath, Type eventType, Type? consumerType = null, string? subscriptionName = null)
    {
        TopicMappings.TryAdd(eventType, topicPath);
        
        if (consumerType != null && !string.IsNullOrWhiteSpace(subscriptionName))
        {
            Subscriptions.Add(new Subscription(topicPath, consumerType, subscriptionName));
        }
        
        return this;
    }
    
    public EventBusOptions AddPublishMapping(Type eventType, string topic)
    {
        TopicMappings.TryAdd(eventType, topic);

        return this;
    }
    
    public EventBusOptions AddSubscription(string topicPath, Type consumerType, string subscriptionName)
    {
        Subscriptions.Add(new Subscription(topicPath, consumerType, subscriptionName));

        return this;
    }

    public EventBusOptions RetryImmediately(int retryCount)
    {
        ImmediateRetry = true;
        RetryCount = retryCount;

        return this;
    }

    public EventBusOptions SetRetryIntervals(params TimeSpan[] retryIntervals)
    {
        if (retryIntervals.Length > 0)
        {
            RetryIntervals = retryIntervals;
            ImmediateRetry = false;
            RetryCount = retryIntervals.Length;
        }

        return this;
    }
    
    public EventBusOptions AddConsumerAssemblies(params Assembly[] assemblies)
    {
        if (assemblies.Length > 0)
        {
            Assemblies.AddRange(assemblies);
        }

        return this;
    }
}

public enum BusType 
{
    InMemory,
    AzureServiceBus,
    RabbitMq
}