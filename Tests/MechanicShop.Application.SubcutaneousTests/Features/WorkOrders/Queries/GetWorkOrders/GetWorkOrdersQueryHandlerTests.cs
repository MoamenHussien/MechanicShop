using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrdersQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetWorkOrdersQueryHandler_ShouldSucceed_AndReturnPaginatedList()
    {
        // Arrange
        var query = new GetAllWorkOrderQuery(
            PageIndex: 1,
            PageSize: 10,
            SearchTerm: null);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.TotalCount >= 0);
    }

    [Fact]
    public async Task GetWorkOrdersQueryHandler_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var vehicleModel = await _context.VehicleModels.FirstAsync();
        var repairTask = await _context.RepairTasks.FirstAsync();

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(10);
        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt.AddDays(1),
            endAt: startAt.AddDays(1).AddHours(2),
            repairTasks: [repairTask]).Value;

        workOrder1.MarkAsCancelled();

        await _context.WorkOrders.AddAsync(workOrder1);
        await _context.WorkOrders.AddAsync(workOrder2);
        await _context.SaveChangesAsync(default);

        // Filter by Cancelled State
        var query = new GetAllWorkOrderQuery(
            PageIndex: 1,
            PageSize: 10,
            SearchTerm: null,
            State: WorkOrderState.Cancelled);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.TotalCount >= 1);
        Assert.All(result.Value.Items, w => Assert.Equal(WorkOrderState.Cancelled, w.State));
    }
}