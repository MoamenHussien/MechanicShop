using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetWorkOrderByIdQueryHandler_WithInvalidId_ShouldFail()
    {
        // Arrange
        var query = new global::GetWorkOrderByIdQuery(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheWorkOrder.Code, result.TopError.Code);
    }

    [Fact]
    public async Task GetWorkOrderByIdQueryHandler_WithValidId_ShouldSucceed()
    {
        // Arrange
        var vehicleModel = await _context.VehicleModels.FirstAsync();
        var repairTask = await _context.RepairTasks.FirstAsync();

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var query = new global::GetWorkOrderByIdQuery(workOrder.Id);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
        Assert.Equal(Spot.A, result.Value.Spot);
        Assert.Equal(startAt, result.Value.StartAtUtc);
        Assert.NotNull(result.Value.Labor);
        Assert.Equal(employee.Id, result.Value.Labor.LaborId);
        Assert.Single(result.Value.RepairTasks);
    }
}
