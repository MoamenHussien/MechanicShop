using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepaireTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task UpdateRepairTaskHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask(name: "Old Task").Value;

        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);
        ((DbContext)_context).ChangeTracker.Clear();

        var newPartId = Guid.NewGuid();
        var command = new UpdateRepairTaskCommand(
            id: repairTask.Id,
            name: "Updated Task",
            LaborCost: 200m,
            duration: RepairDurationInMinutes.Min60,
            Parts:
            [
                new UpdatePartCommand(newPartId, "New Part", 100m, 2)
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var updatedRepairTask = await _context.RepairTasks
            .Include(r => r.Parts)
            .FirstOrDefaultAsync(r => r.Id == repairTask.Id);

        Assert.NotNull(updatedRepairTask);
        Assert.Equal("Updated Task", updatedRepairTask.Name);
        Assert.Equal(200m, updatedRepairTask.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min60, updatedRepairTask.EstimatedDuration);
        Assert.Single(updatedRepairTask.Parts);
        Assert.Equal("New Part", updatedRepairTask.Parts[0].Name);
        Assert.Equal(100m, updatedRepairTask.Parts[0].Costs);
        Assert.Equal(2, updatedRepairTask.Parts[0].Quantity);
    }

    [Fact]
    public async Task UpdateRepairTaskHandler_WithInvalidRepairTaskId_ShouldFail()
    {
        // Arrange
        var command = new UpdateRepairTaskCommand(
            id: Guid.NewGuid(),
            name: "Updated Task",
            LaborCost: 200m,
            duration: RepairDurationInMinutes.Min60,
            Parts:
            [
                new UpdatePartCommand(Guid.NewGuid(), "New Part", 100m, 2)
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundThisRepairTaskId.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateRepairTaskHandler_WithExistingName_ShouldFail()
    {
        // Arrange
        var existingTask = RepairTaskFactory.CreateRepairTask(name: "Existing Task").Value;
        var repairTaskToUpdate = RepairTaskFactory.CreateRepairTask(name: "Old Task").Value;

        await _context.RepairTasks.AddAsync(existingTask);
        await _context.RepairTasks.AddAsync(repairTaskToUpdate);
        await _context.SaveChangesAsync(default);

        var command = new UpdateRepairTaskCommand(
            id: repairTaskToUpdate.Id,
            name: "Existing Task", // Duplicate name
            LaborCost: 200m,
            duration: RepairDurationInMinutes.Min60,
            Parts:
            [
                new UpdatePartCommand(Guid.NewGuid(), "New Part", 100m, 2)
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.DuplicateName.Code, result.TopError.Code);
    }
}
