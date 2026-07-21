using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MechanicShop.Tests.Common.RepaireTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RelocateWorkOrderCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task RelocateWorkOrderHandler_WithInvalidWorkOrderId_ShouldFail()
    {
        // Arrange
        var command = new RelocateWorkOrderCommand(
            Guid.NewGuid(), 
            DateTimeOffset.UtcNow.AddDays(1), 
            Spot.A);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheWorkOrder.Code, result.TopError.Code);
    }

    [Fact]
    public async Task RelocateWorkOrderHandler_WithSameStartAndSpot_ShouldFail()
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

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(21).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new RelocateWorkOrderCommand(workOrder.Id, startAt, Spot.A);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NothingIsChanged.Code, result.TopError.Code);
    }

    [Fact]
    public async Task RelocateWorkOrderHandler_WithOutsideOperatingHours_ShouldFail()
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

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(22).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(22).AddHours(12),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        // Relocate to 4 AM
        var newStart = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(23).AddHours(4);
        var command = new RelocateWorkOrderCommand(workOrder.Id, newStart, Spot.B);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task RelocateWorkOrderHandler_WithVehicleConflict_ShouldFail()
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

        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);

        var startAt1 = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(24).AddHours(10);
        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee1.Id,
            startAt: startAt1,
            endAt: startAt1.AddHours(2),
            repairTasks: [repairTask]).Value;

        var startAt2 = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(24).AddHours(13);
        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee2.Id,
            startAt: startAt2,
            endAt: startAt2.AddHours(2),
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder1);
        await _context.WorkOrders.AddAsync(workOrder2);
        await _context.SaveChangesAsync(default);

        // Try to relocate WorkOrder1 to overlap with WorkOrder2 (same vehicle)
        var command = new RelocateWorkOrderCommand(workOrder1.Id, startAt2, Spot.C);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.VehicleSchedulingConflict.Code, result.TopError.Code);
    }

    [Fact]
    public async Task RelocateWorkOrderHandler_WithLaborConflict_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer1 = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var customer2 = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer1);
        await _context.Customers.AddAsync(customer2);
        await _context.Employees.AddAsync(employee);

        var startAt1 = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(25).AddHours(10);
        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer1.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt1,
            endAt: startAt1.AddHours(2),
            repairTasks: [repairTask]).Value;

        var startAt2 = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(25).AddHours(13);
        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer2.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt2,
            endAt: startAt2.AddHours(2),
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder1);
        await _context.WorkOrders.AddAsync(workOrder2);
        await _context.SaveChangesAsync(default);

        // Try to relocate WorkOrder1 to overlap with WorkOrder2 (same labor)
        var command = new RelocateWorkOrderCommand(workOrder1.Id, startAt2, Spot.C);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime.Code, result.TopError.Code);
    }

    [Fact]
    public async Task RelocateWorkOrderHandler_WithUnavailableSpot_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer1 = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var customer2 = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]
        ).Value;

        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer1);
        await _context.Customers.AddAsync(customer2);
        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);

        var startAt1 = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(26).AddHours(10);
        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer1.vehicles.First().Id,
            laborId: employee1.Id,
            startAt: startAt1,
            endAt: startAt1.AddHours(2),
            repairTasks: [repairTask]).Value;

        var startAt2 = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(26).AddHours(13);
        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer2.vehicles.First().Id,
            laborId: employee2.Id,
            startAt: startAt2,
            endAt: startAt2.AddHours(2),
            spot: Spot.B,
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder1);
        await _context.WorkOrders.AddAsync(workOrder2);
        await _context.SaveChangesAsync(default);

        // Try to relocate WorkOrder1 to overlap with WorkOrder2 on Spot B
        var command = new RelocateWorkOrderCommand(workOrder1.Id, startAt2, Spot.B);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.RangeTimeIsAlreadyTakenByAnotherWorkOrderAtThisSpot.Code, result.TopError.Code);
    }

    [Fact]
    public async Task RelocateWorkOrderHandler_WithValidData_ShouldSucceed()
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

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(27).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            spot: Spot.A,
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var newStartAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(28).AddHours(10);
        var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Spot.B);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess, result.IsError ? result.TopError.Description : "");

        ((DbContext)_context).ChangeTracker.Clear();

        var updatedWorkOrder = await _context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
        Assert.Equal(newStartAt, updatedWorkOrder.StartAtUtc);
        Assert.Equal(Spot.B, updatedWorkOrder.Spot);
    }

    [Fact]
    public async Task RelocateWorkOrderHandler_WithNonEditableState_ShouldFail()
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

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(29).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            spot: Spot.A,
            repairTasks: [repairTask]).Value;

        workOrder.UpdateTiming(new DateTimeOffset(DateTime.UtcNow.Date).AddDays(-1).AddHours(10), new DateTimeOffset(DateTime.UtcNow.Date).AddDays(-1).AddHours(12));

        // Force state to InProgress (which is not editable)
        workOrder.MarkAsInProgress();

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var newStartAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(30).AddHours(10);
        var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Spot.B);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrderErrors.TimingReadonly");
    }
}