public interface IWorkOrderNotifier
{
    Task NotifyWorkOrdersChangedAsync(CancellationToken ct = default);
}