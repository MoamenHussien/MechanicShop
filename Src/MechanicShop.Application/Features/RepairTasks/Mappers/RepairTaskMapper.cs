public static class RepairTaskMapper
{
    public static PartDto ToDto(this Part part)
    {
        ArgumentNullException.ThrowIfNull(part);
        return new PartDto(part.Id, part.Name, part.Costs, part.Quantity);
    }

    public static List<PartDto> ToDto(this IEnumerable<Part> parts)
    {
        return parts.Select(n => n.ToDto()).ToList();
    }

    public static RepairTaskDto ToDto(this RepairTask repairTask)
    {
        ArgumentNullException.ThrowIfNull(repairTask);
        return new RepairTaskDto
        {
            RepairTaskId = repairTask.Id,
            Name = repairTask.Name,
            LaborCost = repairTask.LaborCost,
            Parts = repairTask.Parts.ToDto(),
            // TotalCost = repairTask.TotalCost,
            EstimatedDurationInMins = repairTask.EstimatedDuration
        };
    }

    public static List<RepairTaskDto> ToDto(this IEnumerable<RepairTask> repairTasks)
    {
        return repairTasks.Select(n => n.ToDto()).ToList();
    }
}
