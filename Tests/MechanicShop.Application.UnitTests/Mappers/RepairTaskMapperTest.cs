using MechanicShop.Tests.Common.RepaireTasks;
using Xunit;

public class RepairTaskMapperTests
{
    [Fact]
    public void SinglePartToDto_WhenPartIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var sourcePart = PartFactory.CreatePart().Value;

        // Act
        var partDto = sourcePart.ToDto();

        // Assert
        Assert.Equal(sourcePart.Id, partDto.PartId);
        Assert.Equal(sourcePart.Name, partDto.Name);
        Assert.Equal(sourcePart.Costs, partDto.Cost);
        Assert.Equal(sourcePart.Quantity, partDto.Quantity);
    }

    [Fact]
    public void GroupPartToDto_WhenPartsAreValid_ShouldMapAllParts()
    {
        // Arrange
        var firstPart = PartFactory.CreatePart().Value;
        var secondPart = PartFactory.CreatePart().Value;

        IEnumerable<Part> sourceParts =
        [
            firstPart,
            secondPart
        ];

        // Act
        var partDtos = sourceParts.ToDto();

        // Assert
        Assert.Equal(sourceParts.Count(), partDtos.Count);

        Assert.Contains(partDtos, dto => dto.PartId == firstPart.Id);
        Assert.Contains(partDtos, dto => dto.PartId == secondPart.Id);
    }

    [Fact]
    public void SingleToDto_WhenRepairTaskIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var sourceRepairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var repairTaskDto = sourceRepairTask.ToDto();

        // Assert
        Assert.Equal(sourceRepairTask.Id, repairTaskDto.RepairTaskId);
        Assert.Equal(sourceRepairTask.Name, repairTaskDto.Name);
        Assert.Equal(sourceRepairTask.LaborCost, repairTaskDto.LaborCost);
        Assert.Equal(sourceRepairTask.EstimatedDuration, repairTaskDto.EstimatedDurationInMins);

        var sourcePart = Assert.Single(sourceRepairTask.Parts);
        var mappedPart = Assert.Single(repairTaskDto.Parts);

        Assert.Equal(sourcePart.Id, mappedPart.PartId);
        Assert.Equal(sourcePart.Name, mappedPart.Name);
        Assert.Equal(sourcePart.Costs, mappedPart.Cost);
        Assert.Equal(sourcePart.Quantity, mappedPart.Quantity);

        Assert.Equal(sourceRepairTask.TotalCost, repairTaskDto.TotalCost);
    }

    [Fact]
    public void GroupToDto_WhenRepairTasksAreValid_ShouldMapAllRepairTasks()
    {
        // Arrange
        var firstRepairTask = RepairTaskFactory.CreateRepairTask().Value;
        var secondRepairTask = RepairTaskFactory.CreateRepairTask().Value;

        IEnumerable<RepairTask> sourceRepairTasks =
        [
            firstRepairTask,
            secondRepairTask
        ];

        // Act
        var repairTaskDtos = sourceRepairTasks.ToDto();

        // Assert
        Assert.Equal(sourceRepairTasks.Count(), repairTaskDtos.Count);

        Assert.Contains(repairTaskDtos, dto => dto.RepairTaskId == firstRepairTask.Id);
        Assert.Contains(repairTaskDtos, dto => dto.RepairTaskId == secondRepairTask.Id);
    }
}
