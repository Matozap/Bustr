# Bustr

![Build](https://img.shields.io/github/actions/workflow/status/Matozap/Location-Microservice/main-build.yml?style=for-the-badge&logo=github&color=0D7EBF)
![Issues](https://img.shields.io/github/issues/Matozap/Location-Microservice?style=for-the-badge&logo=github&color=0D7EBF)
![Commits](https://img.shields.io/github/last-commit/Matozap/Location-Microservice?style=for-the-badge&logo=github&color=0D7EBF)


## Bustr

Bustr is a simplification of MassTransit so you can easily inject the pub-sub pattern into your microservices in an uniform way and 
provides a scalable and flexible solution for handling location information in a distributed system, allowing other services or applications to easily and reliably retrieve location information as needed.

It focuses on topics in case you want to use it with Azure Service Bus and fanout if you use RabbitMQ and it creates both and their respective subscriptions with only a few lines of code 
as showcased below.

------------------------------

### Usage

#### Scenario 1: Producer microservice 

This is the most basic, yet common, scenario in which the application sends an event (`PersonEvent` class) to a topic (`person-event` topic) and has
no subscription to any event from external applications.

```csharp
services.AddBustr(options =>
{
    options.Configure(BusType.RabbitMq, connectionString)
        .RetryImmediately(3)
        .MapTopic("person-event", typeof(PersonEvent));
});
```

###

#### Scenario 2: Producer microservice with async events to itself

This configuration will create 3 topics and 3 subscription (optional) to themselves which is useful for cases were you want the application
to do post-action processing like clearing the cache, sending more messages, send email notifications, etc.

It creates 3 topics under the same directory (`location` as in `location/city-event`) and define which classes are going to consume the subscriptions.

```csharp
services.AddBustr(options =>
{
options.Configure(BusType.AzureServiceBus, connectionString)
.SetRetryIntervals(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10))
.UseDeadLetterQueue(true)
        .MapTopic("location/city-event", typeof(CityEvent), typeof(CityEventConsumer), "self.city.location.service")
        .MapTopic("location/state-event", typeof(StateEvent), typeof(StateEventConsumer), "self.state.location.service")
        .MapTopic("location/country-event", typeof(CountryEvent), typeof(CountryEventConsumer), "self.country.location.service");
});
```

###

#### Scenario 3: Publishing events and consuming events from external applications

This configuration will create 2 topics and 1 subscription to itself, then it will create another just to send events and finally it will subscribe to 2 
events coming from external applications.

```csharp
services.AddBustr(options =>
{
    options.Configure(BusType.RabbitMq, connectionString)
        .RetryImmediately(3)
        .MapTopic("person/payment-event", typeof(CityEvent), typeof(CityEventConsumer), "self.person.service")
        .MapTopic("person/person-event", typeof(PersonEvent))
        .AddSubscription("some-external-topic", typeof(CountryEvent), "census-person-service")
        .AddSubscription("some-other-topic", typeof(DiscountEvent), "discount-person-service");
});
```

###

## Contributing

It is simple, as all things should be:

1. Clone it
2. Improve it
3. Make pull request

## Credits

- Initial development by [Slukad](https://github.com/Slukad)
- MassTransit awesome library by [phatboyg](https://github.com/phatboyg)