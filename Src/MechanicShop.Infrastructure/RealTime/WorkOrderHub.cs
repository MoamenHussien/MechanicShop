using Microsoft.AspNetCore.SignalR;

public sealed class WorkOrderHub : Hub
{
    public const string HubUrl = "/hubs/workorders";

    // ✅ Constant مشترك بين السيرفر والكليينت لتجنب typos
    public const string WorkOrderChangedEvent = "WorkOrderChanged";
}
