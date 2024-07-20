using MassTransit;
using MassTransitExample.Abstract;
using MassTransitExample.Consumers;
using MassTransitExample.Events;
using MassTransitExample.Services;
using System.Net;
using static MassTransit.Logging.DiagnosticHeaders.Messaging;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();



////////////////////////// for Masstransit
builder.Services.AddScoped<IProducerService, ProducerService>();
var queuePrefix = "tenant1";


// Masstransit Non-TLS:
//builder.Services.AddMassTransit(config =>
//{
//    config.AddConsumer<TestEventConsumer>();
//    config.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host(builder.Configuration.GetSection("EventBus")["ConnectionString"]);
//        cfg.ReceiveEndpoint($"{queuePrefix}_{nameof(TestEvent)}", c => { c.ConfigureConsumer<TestEventConsumer>(context); });

//    });
//});


// Masstransit TLS:
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<TestEventConsumer>();
    config.UsingRabbitMq((context, cfg) =>
    {
        config.AddConsumer<TestEventConsumer>();
        var rabbitMqTlsConfig = builder.Configuration.GetSection("RabbitMqTlsConfig");
        cfg.Host(new Uri(rabbitMqTlsConfig["RabbitMqRootUri"]), h =>
        {
            h.Username(rabbitMqTlsConfig["UserName"]);
            h.Password(rabbitMqTlsConfig["Password"]);
            h.UseSsl(s =>
            {
                s.Protocol = SslProtocols.Tls12;
                s.ServerName = rabbitMqTlsConfig["ServerCertCommonName"];
                s.AllowPolicyErrors(SslPolicyErrors.RemoteCertificateChainErrors);
                s.Certificate = GetCertificate(rabbitMqTlsConfig["ClientCertPath"], rabbitMqTlsConfig["ClientCertPassword"]);
            });
        });
        cfg.ReceiveEndpoint($"{queuePrefix}_{nameof(TestEvent)}", c => { c.ConfigureConsumer<TestEventConsumer>(context); });

    });
});

static X509Certificate2 GetCertificate(string pemFilePath, string password)
{
    X509Certificate2 cer = new X509Certificate2(pemFilePath, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
    return cer;
}
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


