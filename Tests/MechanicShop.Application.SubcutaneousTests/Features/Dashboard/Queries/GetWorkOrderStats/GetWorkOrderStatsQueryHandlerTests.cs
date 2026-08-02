using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStats;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderStatsQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetWorkOrderStatsHandler_WithNoWorkOrders_ShouldReturnEmptyStats()
    {
        // Arrange
        // Ensure clean state for the chosen date
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)); // Pick a future date with no seeded data
        var query = new GetWorkOrderStatsQuery(targetDate);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(targetDate, result.Value.Date);
        Assert.Equal(0, result.Value.Total);
        Assert.Equal(0, result.Value.TotalRevenue);
        Assert.Equal(0, result.Value.UniqueVehicles);
    }

    [Fact]
    public async Task GetWorkOrderStatsHandler_WithExistingWorkOrders_ShouldReturnCalculatedStats()
    {
        // Arrange
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var startOfUtcDay = targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var make = VehicleMakeFactory.CreateVehicleMake().Value;
        var customer = CustomerFactory.CreateCustomer(email: "stats1@localhost.com", vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: make.VehicleModels.First().Id).Value]).Value;
        var vehicle = customer.vehicles.First();

        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;
        var repairTask = MechanicShop.Tests.Common.RepaireTasks.RepairTaskFactory.CreateRepairTask().Value;

        await _context.VehicleMakes.AddAsync(make);
        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: employee.Id,
            repairTasks: [repairTask],
            startAt: startOfUtcDay.AddHours(10),
            endAt: startOfUtcDay.AddHours(12)).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var query = new GetWorkOrderStatsQuery(targetDate);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(targetDate, result.Value.Date);
        Assert.True(result.Value.Total >= 1);
        Assert.True(result.Value.Scheduled >= 1);
        Assert.True(result.Value.UniqueVehicles >= 1);
    }
}
