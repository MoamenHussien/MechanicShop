using MediatR;

public sealed class WorkOrderCollectionModifiedEventHandler(IWorkOrderNotifier notifier)
: INotificationHandler<WorkOrderCollectionModified>
{
    public Task Handle(WorkOrderCollectionModified notification, CancellationToken cancellationToken)
    {
        return notifier.NotifyWorkOrdersChangedAsync(cancellationToken);
    }
}