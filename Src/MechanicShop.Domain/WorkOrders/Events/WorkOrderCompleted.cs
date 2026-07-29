public sealed class WorkOrderCompleted() : DomainEvents
{
    public Guid WorkOrderId { get; set; }
}
