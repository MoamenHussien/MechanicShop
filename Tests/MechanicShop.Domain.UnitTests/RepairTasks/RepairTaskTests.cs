using MechanicShop.Tests.Common.RepaireTasks;
using Xunit;

public class RepairTaskTests
{
    [Fact]
    public void CreateRepairTask_ShouldSucceed_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = "Brake Inspection";
        const decimal laborCost = 100m;
        const RepairDurationInMinutes duration = RepairDurationInMinutes.Min30;

        List<Part> parts =
        [
            PartFactory.CreatePart(cost: 50m, quantity: 2).Value
        ];

        // Act
        var result = RepairTaskFactory.CreateRepairTask(
            id: id,
            name: name,
            laborCost: laborCost,
            repairDurationInMinutes: duration,
            parts: parts);

        // Assert
        Assert.True(result.IsSuccess);

        var repairTask = result.Value;

        Assert.Equal(id, repairTask.Id);
        Assert.Equal(name, repairTask.Name);
        Assert.Equal(laborCost, repairTask.LaborCost);
        Assert.Equal(duration, repairTask.EstimatedDuration);

        Assert.Single(repairTask.Parts);
        Assert.Equal(100m, repairTask.TotalPartsCost);
        Assert.Equal(200m, repairTask.TotalCost);
    }

    [Fact]
    public void CreateRepairTask_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = RepairTask.Create(
            id: Guid.Empty,
            name: "Brake Inspection",
            LaborCost: 100,
            repairDuration: RepairDurationInMinutes.Min30,
            parts: [PartFactory.CreatePart(name: "Brake pads", cost: 50, quantity: 1).Value]);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateRepairTask_ShouldFail_WithInvalidName(string? value)
    {
        // Act
        var result = RepairTask.Create(
            id: Guid.NewGuid(),
            name: value!,
            LaborCost: 100,
            repairDuration: RepairDurationInMinutes.Min30,
            parts: [PartFactory.CreatePart(name: "Brake pads", cost: 50, quantity: 1).Value]);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateRepairTask_ShouldFail_WithInvalidLaborCost(decimal value)
    {
        // Act
        var result = RepairTaskFactory.CreateRepairTask(laborCost: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.LaborCostInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WithInvalidDuration()
    {
        // Act
        var result = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: (RepairDurationInMinutes)999);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.DurationInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WithInvalidParts()
    {
        // Act
        var result = RepairTask.Create(
            id: Guid.Empty,
            name: "Brake Inspection",
            LaborCost: 100,
            repairDuration: RepairDurationInMinutes.Min30,
            parts: null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            RepairTaskErrors.AtLeastOneRepairTaskPartIsRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WithEmptyParts()
    {
        // Act
        var result = RepairTaskFactory.CreateRepairTask(parts: []);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            RepairTaskErrors.AtLeastOneRepairTaskPartIsRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpdateRepairTask_ShouldSucceed_WithValidData()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var result = repairTask.Update(
            name: "Oil Change",
            LaborCost: 250m,
            repairDuration: RepairDurationInMinutes.Min60);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal("Oil Change", repairTask.Name);
        Assert.Equal(250m, repairTask.LaborCost);
        Assert.Equal(
            RepairDurationInMinutes.Min60,
            repairTask.EstimatedDuration);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateRepairTask_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var result = repairTask.Update(
            name: value!,
            LaborCost: 100m,
            repairDuration: RepairDurationInMinutes.Min30);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateRepairTask_ShouldFail_WithInvalidLaborCost(decimal value)
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var result = repairTask.Update(
            name: "Repair",
            LaborCost: value,
            repairDuration: RepairDurationInMinutes.Min30);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.LaborCostInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateRepairTask_ShouldFail_WithInvalidDuration()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var result = repairTask.Update(
            name: "Repair",
            LaborCost: 100m,
            repairDuration: (RepairDurationInMinutes)999);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.DurationInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void UpSertParts_ShouldSucceed_WithValidData()
    {
        // Arrange
        var oldPart = PartFactory.CreatePart(name: "Filter").Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            parts: [oldPart]).Value;

        var updatedPart = PartFactory.CreatePart(
            id: oldPart.Id,
            name: "Oil Filter",
            cost: 50m,
            quantity: 2).Value;

        var newPart = PartFactory.CreatePart(
            name: "Brake Pad").Value;

        // Act
        var result = repairTask.UpSert([updatedPart, newPart]);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(2, repairTask.Parts.Count);

        Assert.Contains(repairTask.Parts,
            n => n.Id == updatedPart.Id &&
                 n.Name == "Oil Filter");

        Assert.Contains(repairTask.Parts,
            n => n.Id == newPart.Id);
    }

    [Fact]
    public void UpSertParts_ShouldFail_WithInvalidParts()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var result = repairTask.UpSert(null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            RepairTaskErrors.AtLeastOneRepairTaskPartIsRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpSertParts_ShouldFail_WithEmptyParts()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var result = repairTask.UpSert([]);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            RepairTaskErrors.AtLeastOneRepairTaskPartIsRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpSertParts_ShouldRemovePartsNotIncluded()
    {
        // Arrange
        var part1 = PartFactory.CreatePart().Value;
        var part2 = PartFactory.CreatePart().Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            parts: [part1, part2]).Value;

        var incoming = PartFactory.CreatePart(
            id: part2.Id).Value;

        // Act
        var result = repairTask.UpSert([incoming]);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Single(repairTask.Parts);
        Assert.Equal(part2.Id, repairTask.Parts.Single().Id);
    }
}
