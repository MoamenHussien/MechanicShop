public class WorkOrderDto
{
    public Guid WorkOrderId { get; set; }

    public Guid? InvoiceId { get; set; }

    public Spot Spot { get; set; }

    public VehicleDto? Vehicle { get; set; }

    public DateTimeOffset StartAtUtc { get; set; }

    public DateTimeOffset EndAtUtc { get; set; }

    public List<RepairTaskDto> RepairTasks { get; set; } = [];

    public LaborDto? Labor { get; set; }

    public WorkOrderState State { get; set; }

    public decimal TotalPartCost => RepairTasks.SelectMany(rt => rt.Parts).Sum(p => p.Cost * p.Quantity);

    public decimal TotalLaborCost => RepairTasks.Sum(n => n.LaborCost);

    public decimal TotalCost => RepairTasks.Sum(n => n.TotalCost);

    public int TotalDurationInMins => (int)(EndAtUtc - StartAtUtc).TotalMinutes;

    public DateTimeOffset CreatedAt { get; set; }
}
