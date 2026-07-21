

using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepaireTasks;

using MediatR;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateWorkOrderCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task CreateWorkOrderHandler_WithValidData_ShouldSucceed()
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

        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(61)
            .AddHours(10);

        var command = new CreateWorkOrderCommand(
            spot: Spot.B,
            VehicleId: vehicle.Id,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id],
            LaborId: employee.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var workOrder = await _context.WorkOrders
            .Include(w => w.RepairTasks)
            .Include(n => n.Vehicle)
            .SingleAsync(w => w.Id == result.Value.WorkOrderId);

        Assert.Equal(vehicle.Id, workOrder.VehicleId);
        Assert.Equal(employee.Id, workOrder.LaborId);
        Assert.Equal(Spot.B, workOrder.Spot);
        Assert.Equal(vehicleModel.Id, workOrder.Vehicle.VehicleModelId);

        Assert.Single(workOrder.RepairTasks);
        Assert.Equal(repairTask.Id, workOrder.RepairTasks[0].Id);

        Assert.Equal(vehicle.Id, result.Value.Vehicle!.Id);
        Assert.Equal(employee.Id, result.Value.Labor!.LaborId);
        Assert.Equal(Spot.B, result.Value.Spot);

        Assert.Single(result.Value.RepairTasks);
        Assert.Equal(repairTask.Id, result.Value.RepairTasks[0].RepairTaskId);
    }

    [Fact]
    public async Task CreateWorkOrderHandler_WithMissingRepairTask_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        await _context.VehicleMakes.AddAsync(vehicleMake);

        var customer = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var vehicle = customer.vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(62)
            .AddHours(11);

        var command = new CreateWorkOrderCommand(
            LaborId: employee.Id,
            VehicleId: vehicle.Id,
            spot: Spot.C,
            StartAtUtc: scheduledAt,
            repairTasksIds: [Guid.NewGuid()]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundAnyRepairTasks.Code, result.TopError.Code);
    }



    [Fact]
    public async Task CreateWorkOrderHandler_WithOutsideOperatingHours_ShouldFail()
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

        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(63)
            .AddHours(4);

        var command = new CreateWorkOrderCommand(
            LaborId: employee.Id,
            VehicleId: vehicle.Id,
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }




    [Fact]
    public async Task CreateWorkOrderHandler_WithShortDuration_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min15).Value;
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

        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(64)
            .AddHours(12);

        var command = new CreateWorkOrderCommand(
            LaborId: employee.Id,
            VehicleId: vehicle.Id,
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder_TooShort");
    }





    [Fact]
    public async Task CreateWorkOrderHandler_WithMissingVehicle_ShouldFail()
    {
        // Arrange
        var repairTask = await _context.RepairTasks.FirstAsync();
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
            .AddDays(1)
            .AddHours(13);

        var command = new CreateWorkOrderCommand(
            LaborId: employee.Id,
            VehicleId: Guid.NewGuid(),
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }



    [Fact]
    public async Task CreateWorkOrderHandler_WithMissingLabor_ShouldFail()
    {
        // Arrange
        var vehicleModel = await _context.VehicleModels.FirstAsync();
        var repairTask = await _context.RepairTasks.FirstAsync();

        var customer = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var vehicle = customer.vehicles.First();

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(66)
            .AddHours(14);

        var command = new CreateWorkOrderCommand(
            LaborId: Guid.NewGuid(),
            VehicleId: vehicle.Id,
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }



    [Fact]
    public async Task CreateWorkOrderHandler_WithVehicleConflict_ShouldFail()
    {
        // Arrange
        var vehicleModel = await _context.VehicleModels.FirstAsync();
        var repairTask = await _context.RepairTasks.FirstAsync();

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
        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(67)
            .AddHours(11);

        var command1 = new CreateWorkOrderCommand(
            LaborId: employee1.Id,
            VehicleId: vehicle.Id,
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        var command2 = new CreateWorkOrderCommand(
            LaborId: employee2.Id,
            VehicleId: vehicle.Id,
            spot: Spot.C,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        await _mediator.Send(command1);

        // Act
        var result = await _mediator.Send(command2);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.VehicleSchedulingConflict.Code, result.TopError.Code);
    }


    [Fact]
    public async Task CreateWorkOrderHandler_WithLaborConflict_ShouldFail()
    {
        // Arrange
        var vehicleModel = await _context.VehicleModels.FirstAsync();
        var repairTask = await _context.RepairTasks.FirstAsync();

        var customer1 = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var customer2 = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var vehicle1 = customer1.vehicles.First();
        var vehicle2 = customer2.vehicles.First();

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer1);
        await _context.Customers.AddAsync(customer2);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(68)
            .AddHours(12);

        var command1 = new CreateWorkOrderCommand(
            LaborId: employee.Id,
            VehicleId: vehicle1.Id,
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        var command2 = new CreateWorkOrderCommand(
            LaborId: employee.Id,
            VehicleId: vehicle2.Id,
            spot: Spot.C,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        await _mediator.Send(command1);

        // Act
        var result = await _mediator.Send(command2);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime.Code, result.TopError.Code);
    }



    [Fact]
    public async Task CreateWorkOrderHandler_WithUnavailableSpot_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer1 = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var customer2 = CustomerFactory.CreateCustomer(
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        var vehicle1 = customer1.vehicles.First();
        var vehicle2 = customer2.vehicles.First();

        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer1);
        await _context.Customers.AddAsync(customer2);

        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);

        await _context.SaveChangesAsync(default);

        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.Date)
            .AddDays(69)
            .AddHours(13);

        var command1 = new CreateWorkOrderCommand(
            LaborId: employee1.Id,
            VehicleId: vehicle1.Id,
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        var command2 = new CreateWorkOrderCommand(
            LaborId: employee2.Id,
            VehicleId: vehicle2.Id,
            spot: Spot.B,
            StartAtUtc: scheduledAt,
            repairTasksIds: [repairTask.Id]);

        await _mediator.Send(command1);

        // Act
        var result = await _mediator.Send(command2);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            ApplicationErrors.RangeTimeIsAlreadyTakenByAnotherWorkOrderAtThisSpot.Code,
            result.TopError.Code);
    }
}

