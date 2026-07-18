public static class WorkOrderMapping
{
    public static WorkOrderDto ToDto(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);
        return new WorkOrderDto
        {
            WorkOrderId = workOrder.Id,
            InvoiceId = workOrder.Invoice?.Id,
            Spot = workOrder.Spot,
            Vehicle = workOrder.Vehicle?.ToDto(),
            StartAtUtc = workOrder.StartAtUtc,
            EndAtUtc = workOrder.EndAtUtc,
            RepairTasks = workOrder.RepairTasks.ToDto(),
            Labor = workOrder.Labor?.ToDto(),
            State = workOrder.State,
            CreatedAt = workOrder.CreatedAtUtc
        };
    }
    public static List<WorkOrderDto> ToDto(this List<WorkOrder> workOrders)
    {
        ArgumentNullException.ThrowIfNull(workOrders);
        return workOrders.Select(ToDto).ToList();
    }
    

    public static WorkOrderListItemDto ToListItemDto(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);
        return new WorkOrderListItemDto
        {
            WorkOrderId = workOrder.Id,
            InvoiceId = workOrder.Invoice?.Id,
            Spot = workOrder.Spot,
            Vehicle = workOrder.Vehicle.ToDto(),
            StartAtUtc = workOrder.StartAtUtc,
            EndAtUtc = workOrder.EndAtUtc,
            RepairTasks = workOrder.RepairTasks.Select(n => n.Name).ToList(),
            Labor = workOrder.Labor?.FullName,
            State = workOrder.State,
            Customer = workOrder.Vehicle.Customer?.Name
        };
    }

}

