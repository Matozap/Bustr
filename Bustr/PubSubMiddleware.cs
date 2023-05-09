using System.Reflection;
using Bustr.Bus;
using Bustr.Core.Subscription;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Bustr;

public static class PubSubMiddleware
{
    public static IServiceCollection AddBustr(this IServiceCollection services, Action<EventBusOptions> options)
    {
        var eventBusOptions = new EventBusOptions();
        options.Invoke(eventBusOptions);
        var subscriptionConfiguration = new SubscriptionBuilder(eventBusOptions);
        
        if (!eventBusOptions.Disabled)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumers(entryAssembly);

                if (eventBusOptions.Assemblies.Count > 0)
                {
                    foreach (var assembly in eventBusOptions.Assemblies)
                    {
                        x.AddConsumers(assembly);
                    }
                }

                switch (eventBusOptions.BusType)
                {
                    case BusType.InMemory:
                    default:
                        x.UsingInMemory((context, cfg) =>
                        {
                            if (eventBusOptions.TopicMappings.Count > 0)
                            {
                                foreach (var (type, topicName) in eventBusOptions.TopicMappings)
                                {
                                    subscriptionConfiguration.MapEventToTopic(type, topicName, inMemoryBusFactoryConfigurator: cfg, serviceBusBusFactoryConfigurator: null, rabbitMqBusFactoryConfigurator: null);
                                }
                            }

                            cfg.ConfigureEndpoints(context);
                        });
                        break;
                    
                    case BusType.AzureServiceBus:
                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            cfg.Host(eventBusOptions.ConnectionString);

                            if (eventBusOptions.TopicMappings.Count > 0)
                            {
                                foreach (var (type, topicName) in eventBusOptions.TopicMappings)
                                {
                                    subscriptionConfiguration.MapEventToTopic(type, topicName, inMemoryBusFactoryConfigurator: null, serviceBusBusFactoryConfigurator: cfg, rabbitMqBusFactoryConfigurator: null);
                                }
                            }

                            if (eventBusOptions.Subscriptions.Count > 0)
                            {
                                subscriptionConfiguration.RegisterSubscriptionDynamically(context, eventBusOptions.Subscriptions, cfg);
                            }

                            cfg.ConfigureEndpoints(context);
                        });
                        break;
                    case BusType.RabbitMq:
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(new Uri(eventBusOptions.ConnectionString ?? ""));

                            if (eventBusOptions.TopicMappings.Count > 0)
                            {
                                foreach (var (type, topicName) in eventBusOptions.TopicMappings)
                                {
                                    subscriptionConfiguration.MapEventToTopic(type, topicName, inMemoryBusFactoryConfigurator: null, serviceBusBusFactoryConfigurator: null, rabbitMqBusFactoryConfigurator: cfg);
                                }
                            }
                            
                            if (eventBusOptions.Subscriptions.Count > 0)
                            {
                                subscriptionConfiguration.RegisterSubscriptionDynamically(context, eventBusOptions.Subscriptions, cfg);
                            }

                            cfg.ConfigureEndpoints(context);
                        });
                        break;
                }
            });
        }
        
        services.AddSingleton(eventBusOptions);
        services.AddScoped<IEventBus, EventBus>();
        return services;
    }
}
