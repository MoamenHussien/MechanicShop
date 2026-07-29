using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.SignalR;

public class SignalRWorkOrderNotifier(IHubContext<WorkOrderHub> hub) : IWorkOrderNotifier
{
    public async Task NotifyWorkOrdersChangedAsync(CancellationToken ct = default)
    {
        await hub.Clients.All.SendAsync("WorkOrderChanged", cancellationToken: ct);
    }
}
