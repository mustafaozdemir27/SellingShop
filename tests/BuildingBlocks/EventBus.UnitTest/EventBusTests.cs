using EventBus.UnitTest.Events.EventHandlers;
using EventBus.UnitTest.Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventBus.Factory;
using EventBus.Base.Abstraction;
using EventBus.Base;

namespace EventBus.UnitTest
{
    [TestClass]
    public sealed class EventBusTests
    {
        private ServiceCollection services;

        public EventBusTests()
        {
            this.services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
        }

        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });

            var serviceProvider = services.BuildServiceProvider();
            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        }

        //[TestMethod]
        //public void subscribe_event_on_azure_test()
        //{
        //    services.AddSingleton<IEventBus>(sp =>
        //    {
        //        return EventBusFactory.Create(GetAzureConfig(), sp);
        //    });

        //    var serviceProvider = services.BuildServiceProvider();
        //    var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        //    eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

        ////    eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        
        // Task.Delay(2000).Wait();
        //}

        [TestMethod]
        public void send_message_to_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });

            var serviceProvider = services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        }

        //[TestMethod]
        //public void send_message_to_azure_test()
        //{
        //    services.AddSingleton<IEventBus>(sp =>
        //    {
        //        return EventBusFactory.Create(GetAzureConfig(), sp);
        //    });

        //    var serviceProvider = services.BuildServiceProvider();

        //    var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        //    eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        //}

        [TestMethod]
        public void consume_ordercreated_from_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });

            var serviceProvider = services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();


        }

        private EventBusConfig GetAzureConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "UnitTestTopicName",
                EventBusType = EventBusType.AzureServiceBus,
                EventNamePrefix = "IntegrationEvent",
                EventBusConnectionString = "***"
            };
        }

        private EventBusConfig GetRabbitMQConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "UnitTestTopicName",
                EventBusType = EventBusType.RabbitMQ,
                EventNamePrefix = "IntegrationEvent",
            };
        }
    }
}
