using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MechanicShop.Tests.Common.RepaireTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.ReAssignLabor;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ReAssignLaborCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task ReAssignLaborHandler_WithInvalidWorkOrderId_ShouldFail()
    {
        // Arrange
        var command = new ReAssignLaborCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheWorkOrder.Code, result.TopError.Code);
    }

    [Fact]
    public async Task ReAssignLaborHandler_WithMissingLabor_ShouldFail()
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
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(41).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(41).AddHours(12),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new ReAssignLaborCommand(workOrder.Id, Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheLabor.Code, result.TopError.Code);
    }

    [Fact]
    public async Task ReAssignLaborHandler_WithSameLabor_ShouldFail()
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
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(42).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(42).AddHours(12),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new ReAssignLaborCommand(workOrder.Id, employee.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NothingIsChanged.Code, result.TopError.Code);
    }

    [Fact]
    public async Task ReAssignLaborHandler_WithLaborConflict_ShouldFail()
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
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);

        var workOrder1 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: employee1.Id,
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(43).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(43).AddHours(12),
            repairTasks: [repairTask]).Value;

        var workOrder2 = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: employee2.Id,
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(43).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(43).AddHours(12),
            repairTasks: [repairTask],
            spot: Spot.B).Value;

        await _context.WorkOrders.AddAsync(workOrder1);
        await _context.WorkOrders.AddAsync(workOrder2);
        await _context.SaveChangesAsync(default);

        // Try to assign workOrder1 to employee2, who is already busy at this exact time with workOrder2
        var command = new ReAssignLaborCommand(workOrder1.Id, employee2.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime.Code, result.TopError.Code);
    }

    [Fact]
    public async Task ReAssignLaborHandler_WithValidData_ShouldSucceed()
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
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: employee1.Id,
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(44).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(44).AddHours(12),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new ReAssignLaborCommand(workOrder.Id, employee2.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess, result.IsError ? result.TopError.Description : "");

        ((DbContext)_context).ChangeTracker.Clear();

        var updatedWorkOrder = await _context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
        Assert.Equal(employee2.Id, updatedWorkOrder.LaborId);
    }

    [Fact]
    public async Task ReAssignLaborHandler_WithNonEditableState_ShouldFail()
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
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: employee1.Id,
            startAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(45).AddHours(10),
            endAt: new DateTimeOffset(DateTime.UtcNow.Date).AddDays(45).AddHours(12),
            repairTasks: [repairTask]).Value;

        workOrder.UpdateTiming(new DateTimeOffset(DateTime.UtcNow.Date).AddDays(-1).AddHours(10), new DateTimeOffset(DateTime.UtcNow.Date).AddDays(-1).AddHours(12));


        // Force state to InProgress (which is not editable)
        workOrder.MarkAsInProgress();

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new ReAssignLaborCommand(workOrder.Id, employee2.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder Status: InProgress");
    }
}
