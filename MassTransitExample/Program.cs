using MassTransit;
using MassTransitExample.Abstract;
using MassTransitExample.Consumers;
using MassTransitExample.Events;
using MassTransitExample.Extensions;
using MassTransitExample.Services;

var builder = WebApplication.CreateBuilder(args);


////////////////////////// for Masstransit
builder.Services.AddScoped<IProducerService, ProducerService>();


// Masstransit Non-TLS with connection string:
//builder.Services.AddMassTransit(config =>
//{
//    var queuePrefix = "tenant1";
//    config.AddConsumer<TestEventConsumer>();
//    config.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host(builder.Configuration.GetSection("EventBus")["ConnectionString"]);
//        cfg.ReceiveEndpoint($"{queuePrefix}_{nameof(TestEvent)}", c => { c.ConfigureConsumer<TestEventConsumer>(context); });

//    });
//});


// Masstransit TLS with option to disable:
builder.Services.AddMassTransitService(builder.Configuration);


var app = builder.Build();



////////////////////////// for Masstransit Test publishing:
using (var scope = app.Services.CreateScope())
{
    var producerService = scope.ServiceProvider.GetRequiredService<IProducerService>();
    await producerService.SendToTenant<TestEvent>(new TestEvent() { EventData = "test" }, "tenant1");
}
//////////////////////////


app.Run();


