using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepaireTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTaskByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetRepairTaskByIdQueryHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var query = new GetRepairTaskByIdQuery(repairTask.Id);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(repairTask.Id, result.Value.RepairTaskId);
        Assert.Equal(repairTask.Name, result.Value.Name);
        Assert.Equal(repairTask.LaborCost, result.Value.LaborCost);
        Assert.Equal(repairTask.EstimatedDuration, result.Value.EstimatedDurationInMins);
    }

    [Fact]
    public async Task GetRepairTaskByIdQueryHandler_WithInvalidId_ShouldFail()
    {
        // Arrange
        var query = new GetRepairTaskByIdQuery(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundThisRepairTaskId.Code, result.TopError.Code);
    }
}
