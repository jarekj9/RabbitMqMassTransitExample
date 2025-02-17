using MassTransit;
using MassTransitExample.Consumers;
using MassTransitExample.Events;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MassTransitExample.Extensions
{
    public static class MassTransitServiceExtensions
    {
        public static void AddMassTransitService(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var queuePrefix = "tenant1";
            var rabbitMqConfig = configuration.GetSection("RabbitMqConfig");

            serviceCollection.AddMassTransit(config =>
            {
                config.AddConsumer<TestEventConsumer>();
                config.UsingRabbitMq((context, cfg) =>
                {
                    ConfigureRabbitMqAuth(cfg, rabbitMqConfig);
                    cfg.ReceiveEndpoint($"{queuePrefix}_{nameof(TestEvent)}", c => { c.ConfigureConsumer<TestEventConsumer>(context); });

                });
            });

        }
        private static X509Certificate2 GetCertificate(string pemFilePath, string password) =>
            new X509Certificate2(pemFilePath, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

        private static void ConfigureRabbitMqAuth(IRabbitMqBusFactoryConfigurator cfg, IConfigurationSection rabbitMqConfig)
        {
            if (rabbitMqConfig.GetValue<bool>("UseTls"))
            {
                cfg.Host(new Uri(rabbitMqConfig["RabbitMqRootUriTLS"]), h =>
                {
                    h.Username(rabbitMqConfig["UserName"]);
                    h.Password(rabbitMqConfig["Password"]);
                    h.UseSsl(s =>
                    {
                        s.Protocol = SslProtocols.Tls12;
                        s.ServerName = rabbitMqConfig["ServerCertCommonName"];
                        s.AllowPolicyErrors(SslPolicyErrors.RemoteCertificateChainErrors);
                        s.Certificate = GetCertificate(rabbitMqConfig["ClientCertPath"], rabbitMqConfig["ClientCertPassword"]);
                    });
                });
            }
            else
            {
                cfg.Host(new Uri(rabbitMqConfig["RabbitMqRootUri"]), h =>
                {
                    h.Username(rabbitMqConfig["UserName"]);
                    h.Password(rabbitMqConfig["Password"]);
                });
            }
        }
    }
}
