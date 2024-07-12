using MassTransit;
using MassTransitExample.Abstract;
using MassTransitExample.Consumers;
using MassTransitExample.Events;
using MassTransitExample.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();



////////////////////////// for Masstransit
builder.Services.AddScoped<IProducerService, ProducerService>();
var queuePrefix = "tenant1";
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<TestEventConsumer>();
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetSection("EventBus")["ConnectionString"]);
        cfg.ReceiveEndpoint($"{queuePrefix}_{nameof(TestEvent)}", c => { c.ConfigureConsumer<TestEventConsumer>(context); });

    });
});
//////////////////////////



var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//   app.UseSwagger();
//   app.UseSwaggerUI();
//}


////////////////////////// for Masstransit Test publishing:
using (var scope = app.Services.CreateScope())
{
    var producerService = scope.ServiceProvider.GetRequiredService<IProducerService>();
    await producerService.SendToTenant<TestEvent>(new TestEvent() { EventData = "test" }, "tenant1");
}
//////////////////////////


app.Run();


