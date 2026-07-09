using System.ComponentModel;

public class RepairTaskDto
{
    public Guid id { get; set; } 
    public string name { get; set; } = null!;
    public decimal LaborCost { get; set; }
    public List<PartDto> Parts { get; set; } = null!;

}