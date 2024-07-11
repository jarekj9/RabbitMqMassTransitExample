using MassTransitExample.Events;

namespace MassTransitExample.Abstract
{
    public interface IProducerService
    {
        Task SendToTenant<TEvent>(TEvent @event, string tenantCode) where TEvent : IntegrationBaseEvent;
    }
}
