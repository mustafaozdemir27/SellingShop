using EventBus.Base.Abstraction;
using EventBus.Base;
using EventBus.Factory;
using Microsoft.OpenApi.Models;
using PaymentService.Api.IntegrationEvents.EventHandlers;
using PaymentService.Api.IntegrationEvents.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.AddDebug();
});

builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();

builder.Services.AddSingleton<IEventBus>(sp =>
{
    EventBusConfig config = new()
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "PaymentService",
        EventBusType = EventBusType.RabbitMQ
    };

    return EventBusFactory.Create(config, sp);
});


builder.Services.AddControllers();

// OpenAPI (Swagger) configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Service API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API V1");
    });
}

app.UseAuthorization();

app.MapControllers();

IEventBus eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

app.Run();
