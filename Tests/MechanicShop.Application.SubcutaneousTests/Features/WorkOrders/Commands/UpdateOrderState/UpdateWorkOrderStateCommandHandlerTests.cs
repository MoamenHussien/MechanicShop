using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MechanicShop.Tests.Common.RepaireTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateOrderState;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderStateCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task UpdateWorkOrderStateHandler_WithInvalidWorkOrderId_ShouldFail()
    {
        // Arrange
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheWorkOrder.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateWorkOrderStateHandler_WithSameState_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(13).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        workOrder.UpdateTiming(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddHours(2));

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        // Initial state is Scheduled
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.Scheduled);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NothingIsChanged.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateWorkOrderStateHandler_WithStartTimeNotComing_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(11).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        // Future start time, try to mark as InProgress
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.StartTimeNotComing");
    }

    [Fact]
    public async Task UpdateWorkOrderStateHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(12).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        // Cancelled is allowed regardless of start time
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.Cancelled);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess, result.IsError ? result.TopError.Description : "");

        ((DbContext)_context).ChangeTracker.Clear();

        var updatedWorkOrder = await _context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
        Assert.Equal(WorkOrderState.Cancelled, updatedWorkOrder.State);
    }

    [Fact]
    public async Task UpdateWorkOrderStateHandler_WithInvalidStateTransition_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(14).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        workOrder.UpdateTiming(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddHours(2));

        // WorkOrder is initially Scheduled.
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        // Attempting to jump directly from Scheduled to Completed should fail domain validation.
        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.Completed);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Invalid WorkOrder State");
    }
}
