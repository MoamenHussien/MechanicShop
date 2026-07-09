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
            TotalPartCost = workOrder.TotalPartsCost,
            TotalLaborCost = workOrder.TotalLaborCost,
            TotalCost = workOrder.Total,
            TotalDurationInMins = (int)(workOrder.EndAtUtc - workOrder.StartAtUtc).TotalMinutes,
            CreatedAt = workOrder.CreatedAtUtc
        };
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

