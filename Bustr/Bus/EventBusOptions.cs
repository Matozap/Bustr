using System.Reflection;

namespace Bustr.Bus;

public class EventBusOptions
{
    internal record Subscription(string TopicPath, Type ConsumerType, string SubscriptionName);
    /// <summary>
    /// Contains the connection string when the bus type is not InMemory
    /// </summary>
    internal string? ConnectionString { get; private set; }
    /// <summary>
    /// Contains the Access Key when the bus type is Amazon Sqs
    /// </summary>
    internal string? AccessKey { get; private set; }
    /// <summary>
    /// Contains the Secret Key when the bus type is Amazon Sqs
    /// </summary>
    internal string? SecretKey { get; private set; }
    /// <summary>
    /// Contains the Region when the bus type is Amazon Sqs
    /// </summary>
    internal string? Region { get; private set; }
    /// <summary>
    /// Contains the bus type - Use the Configure method to set it
    /// </summary>
    public BusType BusType { get; private set; }
    /// <summary>
    /// Gets if the bus is disabled - Use the DisableBus method to set it
    /// </summary>
    public bool Disabled { get; private set; }
    /// <summary>
    /// Contains the topic mapping dictionary - Use MapTopic or AddPublishMapping methods to set it
    /// </summary>
    public Dictionary<Type, string> TopicMappings { get; } = new();
    /// <summary>
    /// Gets if the bus is set for immediate retries - Use the RetryImmediately method to set it
    /// </summary>
    public bool ImmediateRetry { get; private set; } = true;
    /// <summary>
    /// Contains the retry count for immediate or interval retries - Use the RetryImmediately or SetRetryIntervals method to set it
    /// </summary>
    public int RetryCount { get; private set; } = 2;
    /// <summary>
    /// Gets if the dead letter queue is enabled - Use the UseDeadLetterQueue method to set it
    /// </summary>
    public bool DeadLetterQueueEnabled { get; private set; }
    /// <summary>
    /// Contains the retry intervals - Use the SetRetryIntervals method to set it
    /// </summary>
    internal TimeSpan[]? RetryIntervals { get; private set; }
    /// <summary>
    /// Contains the list of subscription for the application - Use the AddSubscription method to set them
    /// </summary>
    internal List<Subscription> Subscriptions { get; } = new();
    /// <summary>
    /// Contains the list of assemblies to search for consumers - Use the AddConsumerAssemblies method to set them
    /// </summary>
    internal List<Assembly> Assemblies { get; } = new();
    
    /// <summary>
    /// Configures the bus type and optionally sets the connection string - For Amazon SQS use the method overload with region, access key and secret key
    /// </summary>
    /// <param name="busType">The bus type to be used</param>
    /// <param name="connectionString">The connection string which is required when the bus type is not InMemory</param>
    /// <returns>EventBusOptions</returns>
    public EventBusOptions Configure(BusType busType, string? connectionString = null)
    {
        BusType = busType;
        ConnectionString = connectionString;

        if (BusType != BusType.InMemory && string.IsNullOrEmpty(ConnectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "The connection string for the Event bus needs to be set if the bus type is not InMemory");
        }
        
        if (BusType == BusType.AmazonSqs)
        {
            throw new ArgumentNullException(nameof(connectionString), "When using Amazon SQS you need to use the Configure overload passing the region, access key and secret key.");
        }
        
        return this;
    }
    
    /// <summary>
    /// Configures the bus type and optionally sets the region, access key and secret key for managed cloud services like Amazon SQS
    /// </summary>
    /// <param name="busType">The bus type to be used</param>
    /// <param name="region">The cloud region</param>
    /// <param name="accessKey">The access key</param>
    /// <param name="secretKey">The secret key</param>
    /// <returns>EventBusOptions</returns>
    /// <exception cref="ArgumentOutOfRangeException">Bus type need to be Amazon SQS to use this method overload</exception>
    /// <exception cref="ArgumentNullException">Parameters cannot be null or empty</exception>
    public EventBusOptions Configure(BusType busType, string region, string accessKey, string secretKey)
    {
        BusType = busType;
        if (BusType != BusType.InMemory && BusType != BusType.AmazonSqs)
        {
            throw new ArgumentOutOfRangeException(nameof(busType), "To use region, access key and secret key, the bus type must be Amazon SQS ");
        }
        
        if (BusType == BusType.AmazonSqs && string.IsNullOrEmpty(region))
        {
            throw new ArgumentNullException(nameof(region), "Region for the Event bus needs to be set if the bus type is not InMemory");
        }
        
        if (BusType == BusType.AmazonSqs && string.IsNullOrEmpty(accessKey))
        {
            throw new ArgumentNullException(nameof(accessKey), "AccessKey for the Event bus needs to be set if the bus type is not InMemory");
        }
        
        if (BusType == BusType.AmazonSqs && string.IsNullOrEmpty(secretKey))
        {
            throw new ArgumentNullException(nameof(secretKey), "SecretKey for the Event bus needs to be set if the bus type is not InMemory");
        }

        Region = region;
        AccessKey = accessKey;
        SecretKey = secretKey;
        
        return this;
    }
    
    /// <summary>
    /// Disables the event bus without having to change the code
    /// </summary>
    /// <param name="disable">True to disable or false otherwise</param>
    /// <returns>EventBusOptions</returns>
    public EventBusOptions DisableBus(bool disable)
    {
        Disabled = disable;
        return this;
    }
    
    /// <summary>
    /// Indicates if a Dead Letter Queue is going to be used for unsuccessful messages 
    /// </summary>
    /// <param name="useIt">True to use DQL or false otherwise</param>
    /// <returns>EventBusOptions</returns>
    public EventBusOptions UseDeadLetterQueue(bool useIt)
    {
        DeadLetterQueueEnabled = useIt;
        return this;
    }
    
    /// <summary>
    /// Maps an object type to a topic so, if it gets published, it will be sent to that topic and allows to create a subscription to itself (Post message published)
    /// </summary>
    /// <param name="topicPath">The topic to be mapped to the object type</param>
    /// <param name="eventType">The object type to be mapped to the topic</param>
    /// <param name="consumerType">(Optional) A consumer to be used as subscription to itself</param>
    /// <param name="subscriptionName">(Optional) The name of the subscription to itself</param>
    /// <returns>EventBusOptions</returns>
    public EventBusOptions MapTopic(string topicPath, Type eventType, Type? consumerType = null, string? subscriptionName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicPath);
        
        TopicMappings.TryAdd(eventType, topicPath);
        
        if (consumerType != null && !string.IsNullOrWhiteSpace(subscriptionName))
        {
            Subscriptions.Add(new Subscription(topicPath, consumerType, subscriptionName));
        }
        
        return this;
    }
    
    /// <summary>
    /// Maps an object type to a topic so, if it gets published, it will be sent to that topic
    /// </summary>
    /// <param name="topicPath">The topic to be mapped to the object type</param>
    /// <param name="eventType">The object type to be mapped to the topic</param>
    /// <returns>EventBusOptions</returns>
    public EventBusOptions AddPublishMapping(string topicPath, Type eventType)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicPath);
        
        TopicMappings.TryAdd(eventType, topicPath);

        return this;
    }
    
    /// <summary>
    /// Creates a subscription to a topic which will be handled by the consumer and using the subscription name provided
    /// </summary>
    /// <param name="topicPath">The topic to be subscribed to</param>
    /// <param name="consumerType">The type of the consumer</param>
    /// <param name="subscriptionName">The name of the subscription</param>
    /// <returns>EventBusOptions</returns>
    public EventBusOptions AddSubscription(string topicPath, Type consumerType, string subscriptionName)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicPath);
        
        Subscriptions.Add(new Subscription(topicPath, consumerType, subscriptionName));

        return this;
    }

    /// <summary>
    /// Set the events to be retried immediately
    /// </summary>
    /// <param name="retryCount">The retry count</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Must be a positive integer</exception>
    public EventBusOptions RetryImmediately(int retryCount)
    {
        if (retryCount < 0)
        {
            throw new ArgumentException("Retry count must be zero or a positive integer");
        }
        
        ImmediateRetry = true;
        RetryCount = retryCount;

        return this;
    }

    /// <summary>
    /// Sets the events to be retried according to the TimeSpans provided
    /// </summary>
    /// <param name="retryIntervals">The TimeSpans in which the operation will be retried</param>
    /// <returns>EventBusOptions</returns>
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
    
    /// <summary>
    /// Add to assemblies in which to look for the consumers if they are not located in the entry assembly 
    /// </summary>
    /// <param name="assemblies">The assembly list to look for consumers</param>
    /// <returns>EventBusOptions</returns>
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
    RabbitMq,
    AmazonSqs
}