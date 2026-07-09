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
    public decimal TotalPartCost { get; set; }
    public decimal TotalLaborCost { get; set; }
    public decimal TotalCost { get; set; }
    public int TotalDurationInMins { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}