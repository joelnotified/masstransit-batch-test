using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerService;
using IHost = Microsoft.Extensions.Hosting.IHost;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        string connectionString = Environment.GetEnvironmentVariable("ASB_CONNECTION_STRING");
        Configure(services, connectionString);
        services.AddHostedService<Worker>();
    })
    .Build();
await host.RunAsync();

void Configure(IServiceCollection services, string connectionString)
{
    services.AddMassTransit(busConfigurator =>
    {
        busConfigurator.AddConsumer<TestConsumer1>();
        busConfigurator.AddConsumer<TestConsumer2>();
        busConfigurator.AddConsumer<TestConsumer3>();
        busConfigurator.AddConsumer<TestConsumer4>();
        busConfigurator.AddConsumer<TestConsumer5>();

        busConfigurator.UsingAzureServiceBus((context, serviceBusBusFactoryConfigurator) =>
        {
            serviceBusBusFactoryConfigurator.Host(connectionString);

            ConfigureSubsriptionEndpoint<TestConsumer1>(serviceBusBusFactoryConfigurator, context, "subscriber-1");
            ConfigureSubsriptionEndpoint<TestConsumer2>(serviceBusBusFactoryConfigurator, context, "subscriber-2");
            ConfigureSubsriptionEndpoint<TestConsumer3>(serviceBusBusFactoryConfigurator, context, "subscriber-3");
            ConfigureSubsriptionEndpoint<TestConsumer4>(serviceBusBusFactoryConfigurator, context, "subscriber-4");
            ConfigureSubsriptionEndpoint<TestConsumer5>(serviceBusBusFactoryConfigurator, context, "subscriber-5");
        });
    });
    services.AddMassTransitHostedService(true);
}

void ConfigureSubsriptionEndpoint<TConsumer>(IServiceBusBusFactoryConfigurator serviceBusBusFactoryConfigurator, IBusRegistrationContext context, string subscriptionName)
    where TConsumer : class, IConsumer<Batch<IMyEvent>>
{
    serviceBusBusFactoryConfigurator.SubscriptionEndpoint<IMyEvent>(
        subscriptionName,
        receiveEndpointConfigurator =>
        {
            receiveEndpointConfigurator.LockDuration = TimeSpan.FromMinutes(5);
            receiveEndpointConfigurator.PublishFaults = false;
            receiveEndpointConfigurator.MaxAutoRenewDuration = TimeSpan.FromMinutes(30);
            receiveEndpointConfigurator.UseMessageRetry(r => r.Intervals(500, 2000));
            receiveEndpointConfigurator.PrefetchCount = 1100;

            receiveEndpointConfigurator.ConfigureConsumer<TConsumer>(
                context,
                consumerConfigurator =>
                {
                    consumerConfigurator.Options<BatchOptions>(batchOptions =>
                    {
                        batchOptions.MessageLimit = 100;
                        batchOptions.TimeLimit = TimeSpan.FromSeconds(5);
                        batchOptions.ConcurrencyLimit = 10;
                    });
                });
        });
}

namespace WorkerService
{
    public class TestConsumer1 : IConsumer<Batch<IMyEvent>>
    {
        private readonly Random _random;
        private readonly ILogger<TestConsumer1> _logger;

        public TestConsumer1(ILogger<TestConsumer1> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task Consume(ConsumeContext<Batch<IMyEvent>> context)
        {
            _logger.LogInformation("{name} - Consuming {count}", nameof(TestConsumer1), context.Message.Length);
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(4, 8)));
        }
    }

    public class TestConsumer2 : IConsumer<Batch<IMyEvent>>
    {
        private readonly Random _random;
        private readonly ILogger<TestConsumer2> _logger;

        public TestConsumer2(ILogger<TestConsumer2> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task Consume(ConsumeContext<Batch<IMyEvent>> context)
        {
            _logger.LogInformation("{name} - Consuming {count}", nameof(TestConsumer2), context.Message.Length);
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(4, 8)));
        }
    }

    public class TestConsumer3 : IConsumer<Batch<IMyEvent>>
    {
        private readonly Random _random;
        private readonly ILogger<TestConsumer3> _logger;

        public TestConsumer3(ILogger<TestConsumer3> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task Consume(ConsumeContext<Batch<IMyEvent>> context)
        {
            _logger.LogInformation("{name} - Consuming {count}", nameof(TestConsumer3), context.Message.Length);
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(4, 8)));
        }
    }

    public class TestConsumer4 : IConsumer<Batch<IMyEvent>>
    {
        private readonly Random _random;
        private readonly ILogger<TestConsumer4> _logger;

        public TestConsumer4(ILogger<TestConsumer4> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task Consume(ConsumeContext<Batch<IMyEvent>> context)
        {
            _logger.LogInformation("{name} - Consuming {count}", nameof(TestConsumer4), context.Message.Length);
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(4, 8)));
        }
    }

    public class TestConsumer5 : IConsumer<Batch<IMyEvent>>
    {
        private readonly Random _random;
        private readonly ILogger<TestConsumer5> _logger;

        public TestConsumer5(ILogger<TestConsumer5> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task Consume(ConsumeContext<Batch<IMyEvent>> context)
        {
            _logger.LogInformation("{name} - Consuming {count}", nameof(TestConsumer5), context.Message.Length);
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(4, 8)));
        }
    }

    [EntityName("my-event")]
    public interface IMyEvent
    {
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBus _bus;

        public Worker(
            ILogger<Worker> logger,
            IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var tasks = new List<Task>();
            var count = 50000;
            for (int i = 0; i < count; i++)
            {
                tasks.Add(_bus.Publish<IMyEvent>(new { }));
            }

            await Task.WhenAll(tasks);
        }
    }
}