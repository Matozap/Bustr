using Bustr.Bus;
using Bustr.Tests.Consumers;
using Bustr.Tests.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Bustr.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<RandomEventConsumer>();
        services.AddBustr(options =>
        {
            options.Configure(BusType.InMemory, "")
                .RetryImmediately(3)
                .MapTopic("testing/random-event", typeof(RandomEvent), typeof(RandomEventConsumer), "self.random.event.sub")
                .AddConsumerAssemblies(typeof(Startup).Assembly);
        });
    }
}