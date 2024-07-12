using MassTransit;
using MassTransitExample.Abstract;
using MassTransitExample.Events;

namespace MassTransitExample.Services
{
    public class ProducerService : IProducerService
    {
        private readonly ISendEndpointProvider _endpointProvider;
        private readonly ILogger<ProducerService> _logger;

        public ProducerService(ISendEndpointProvider endpointProvider, ILogger<ProducerService> logger)
        {
            _endpointProvider = endpointProvider;
            _logger = logger;
        }

        public async Task SendToTenant<TEvent>(TEvent @event, string tenantCode) where TEvent : IntegrationBaseEvent
        {
            var eventName = @event.GetType().Name;
            var endpointUri = new Uri($"queue:{tenantCode}_{eventName}");
            var endpoint = await _endpointProvider.GetSendEndpoint(endpointUri);
            _logger.LogInformation($"Sending {eventName} with id: {@event.Id} to endpoint: {endpointUri}");
            await endpoint.Send(@event);
        }
    }
}
