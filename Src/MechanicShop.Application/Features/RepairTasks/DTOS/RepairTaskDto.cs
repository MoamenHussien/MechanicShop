using System.ComponentModel;

public class RepairTaskDto
{
    public Guid RepairTaskId { get; set; }

    public string Name { get; set; } = string.Empty;

    public RepairDurationInMinutes EstimatedDurationInMins { get; set; }

    public decimal LaborCost { get; set; }

    public decimal TotalCost => LaborCost + Parts.Sum(n => n.Cost * n.Quantity);

    public List<PartDto> Parts { get; set; } = [];
}
