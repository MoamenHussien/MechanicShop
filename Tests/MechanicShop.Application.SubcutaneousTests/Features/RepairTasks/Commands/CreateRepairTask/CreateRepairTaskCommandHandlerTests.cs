using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepaireTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateRepairTaskCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task CreateRepairTaskHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new CreateRepairTaskCommand(
            name: "New Repair Task",
            LaborCost: 150m,
            duration: RepairDurationInMinutes.Min60,
            Parts:
            [
                new CreateRepairTaskPartCommand("Brake Pads", 50m, 2)
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var repairTask = await _context.RepairTasks
            .Include(r => r.Parts)
            .SingleAsync(r => r.Id == result.Value.RepairTaskId);

        Assert.Equal("New Repair Task", repairTask.Name);
        Assert.Equal(150m, repairTask.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min60, repairTask.EstimatedDuration);

        Assert.Single(repairTask.Parts);
        Assert.Equal("Brake Pads", repairTask.Parts[0].Name);
        Assert.Equal(50m, repairTask.Parts[0].Costs);
        Assert.Equal(2, repairTask.Parts[0].Quantity);
    }

    [Fact]
    public async Task CreateRepairTaskHandler_WithExistingName_ShouldFail()
    {
        // Arrange
        var existingTask = RepairTaskFactory.CreateRepairTask(name: "Existing Task").Value;

        await _context.RepairTasks.AddAsync(existingTask);
        await _context.SaveChangesAsync(default);

        var command = new CreateRepairTaskCommand(
            name: "Existing Task", // Duplicate name
            LaborCost: 150m,
            duration: RepairDurationInMinutes.Min60,
            Parts:
            [
                new CreateRepairTaskPartCommand("Brake Pads", 50m, 2)
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.DuplicateName.Code, result.TopError.Code);
    }
}