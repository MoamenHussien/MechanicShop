using Microsoft.AspNetCore.SignalR;

public sealed class WorkOrderHub : Hub
{
    public const string HubUrl = "/hubs/workorders";
}
