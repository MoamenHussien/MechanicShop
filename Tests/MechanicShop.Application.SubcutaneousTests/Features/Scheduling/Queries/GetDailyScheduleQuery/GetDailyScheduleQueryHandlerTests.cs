using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Scheduling.Queries.GetDailyScheduleQuery;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetDailyScheduleQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetDailyScheduleQueryHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var scheduleDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var timeZone = TimeZoneInfo.Utc;

        var employee = Employee.Create(Guid.NewGuid(), "John", "Doe").Value;
        var make = VehicleMakeFactory.CreateVehicleMake().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: make.VehicleModels.First().Id).Value]).Value;
        var vehicle = customer.vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.Employees.AddAsync(employee);
        await _context.VehicleMakes.AddAsync(make);
        await _context.Customers.AddAsync(customer);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            laborId: employee.Id,
            vehicleId: vehicle.Id,
            startAt: DateTimeOffset.UtcNow.AddDays(1),
            endAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            spot: Spot.A,
            repairTasks: [repairTask]
        ).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var query = new global::GetDailyScheduleQuery(scheduleDate, timeZone);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(scheduleDate, result.Value.OnDate);
        Assert.NotNull(result.Value.Spots);
        Assert.NotEmpty(result.Value.Spots);
    }
}
