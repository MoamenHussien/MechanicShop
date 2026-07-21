using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MechanicShop.Tests.Common.RepaireTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DeleteWorkOrderCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task DeleteWorkOrderHandler_WithInvalidWorkOrderId_ShouldFail()
    {
        // Arrange
        var command = new DeleteWorkOrderCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheWorkOrder.Code, result.TopError.Code);
    }

    [Fact]
    public async Task DeleteWorkOrderHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var vehicle = customer.vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: employee.Id,
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(50).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(50).AddHours(12),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess, result.IsError ? result.TopError.Description : "");

        ((DbContext)_context).ChangeTracker.Clear();

        var deletedWorkOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrder.Id);
        Assert.Null(deletedWorkOrder);
    }

    [Fact]
    public async Task DeleteWorkOrderHandler_WithNonDeletableState_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var vehicle = customer.vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: employee.Id,
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(51).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(51).AddHours(12),
            repairTasks: [repairTask]).Value;

        workOrder.UpdateTiming(new DateTimeOffset(DateTime.UtcNow.Date).AddDays(-1).AddHours(10), new DateTimeOffset(DateTime.UtcNow.Date).AddDays(-1).AddHours(12));

        // Force state to InProgress (which is not deletable)
        workOrder.MarkAsInProgress();

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder Status: InProgress");
    }
}