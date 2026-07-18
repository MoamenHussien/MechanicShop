
namespace MechanicShop.Tests.Common.RepaireTasks;

public static class RepairTaskFactory
{
    public static Result<RepairTask> CreateRepairTask(
        Guid? id = null,
        string? name = null,
        decimal? laborCost = null,
        RepairDurationInMinutes? repairDurationInMinutes = null,
        List<Part>? parts = null)
    {
        return RepairTask.Create(
            id ?? Guid.NewGuid(),
            name ?? "Brake Inspection",
            laborCost ?? 100,
            repairDurationInMinutes ?? RepairDurationInMinutes.Min30,
            parts ?? [PartFactory.CreatePart(name:"Brake pads",cost:50,quantity:1).Value]);
    }
}