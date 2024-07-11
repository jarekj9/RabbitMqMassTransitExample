namespace MassTransitExample.Events
{
    public class IntegrationBaseEvent
    {
        public Guid Id { get; }
        public DateTime CreationDate { get; }

        public IntegrationBaseEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public IntegrationBaseEvent(Guid id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
        }
    }
}
