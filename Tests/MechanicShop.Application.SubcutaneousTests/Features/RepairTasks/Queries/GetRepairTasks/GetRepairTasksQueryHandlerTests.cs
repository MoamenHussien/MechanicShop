using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepaireTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTasksQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetRepairTasksQueryHandler_WhenRepairTasksExist_ShouldSucceed()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var query = new GetRepairTasksQuery();

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Contains(result.Value, r => r.RepairTaskId == repairTask.Id);
    }

    [Fact]
    public async Task GetRepairTasksQueryHandler_WhenNoRepairTasksExist_ShouldFail()
    {
        // Arrange
        // WebAppFactory InitializeAsync already clears the DB, but we ensure no repair tasks exist.
        _context.RepairTasks.RemoveRange(await _context.RepairTasks.ToListAsync());
        await _context.SaveChangesAsync(default);

        var query = new GetRepairTasksQuery();

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundAnyRepairTasks.Code, result.TopError.Code);
    }
}
