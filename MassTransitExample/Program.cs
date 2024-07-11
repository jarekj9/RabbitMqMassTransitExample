using MassTransit;
using MassTransitExample.Abstract;
using MassTransitExample.Consumers;
using MassTransitExample.Events;
using MassTransitExample.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProducerService, ProducerService>();

var queuePrefix = "queueTenant1";
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<TestEventConsumer>();
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetSection("EventBus")["ConnectionString"]);
        cfg.ReceiveEndpoint(queuePrefix + "_" + nameof(TestEvent), c => { c.ConfigureConsumer<TestEventConsumer>(ctx); });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


using(var scope = app.Services.CreateScope())
{
    var producerService = scope.ServiceProvider.GetRequiredService<IProducerService>();
    await producerService.SendToTenant<TestEvent>(new TestEvent() { EventData = "test" }, "tenant1");
}

app.Run();


