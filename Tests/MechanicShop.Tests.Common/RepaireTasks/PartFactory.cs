

namespace MechanicShop.Tests.Common.RepaireTasks;

public static class PartFactory
{
    public static Result<Part> CreatePart(Guid? id = null, string? name = null, decimal? cost = null, int? quantity = null)
    {
        return Part.Create(
            id ?? Guid.NewGuid(),
            cost ?? 100,
            name ?? "Brake Pad",
            quantity ?? 2);
    }
}
