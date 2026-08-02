using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderRepairTasksCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task UpdateWorkOrderRepairTasksHandler_WithInvalidWorkOrderId_ShouldFail()
    {
        // Arrange
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheWorkOrder.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateWorkOrderRepairTasksHandler_WithSameRepairTasks_ShouldSucceed()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(1).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [repairTask.Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Assert.Equal(ApplicationErrors.NothingIsChanged.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateWorkOrderRepairTasksHandler_WithMissingRepairTask_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddAsync(repairTask);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(2).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [Guid.NewGuid()]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundAnyRepairTasks.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateWorkOrderRepairTasksHandler_WithSomeMissingRepairTask_ShouldFail()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTasks = new List<RepairTask> { RepairTaskFactory.CreateRepairTask().Value, RepairTaskFactory.CreateRepairTask().Value };
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddRangeAsync(repairTasks);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(3).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTasks[0]]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [repairTasks[1].Id, Guid.NewGuid()]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.SomeRepairTaskIdsNotfound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateWorkOrderRepairTasksHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;
        var vehicleModel = vehicleMake.VehicleModels.First();
        var repairTasks = new List<RepairTask> { RepairTaskFactory.CreateRepairTask().Value, RepairTaskFactory.CreateRepairTask().Value };
        await _context.VehicleMakes.AddAsync(vehicleMake);
        await _context.RepairTasks.AddRangeAsync(repairTasks);

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value]).Value;

        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);

        var startAt = new DateTimeOffset(DateTime.UtcNow.Date).AddDays(4).AddHours(10);
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: customer.vehicles.First().Id,
            laborId: employee.Id,
            startAt: startAt,
            endAt: startAt.AddHours(2),
            repairTasks: [repairTasks[0]]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [repairTasks[1].Id]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess, result.IsError ? result.TopError.Description : string.Empty);

        ((DbContext)_context).ChangeTracker.Clear();
        var updatedWorkOrder = await _context.WorkOrders.Include(w => w.RepairTasks).FirstAsync(w => w.Id == workOrder.Id);
        Assert.Single(updatedWorkOrder.RepairTasks);
        Assert.Equal(repairTasks[1].Id, updatedWorkOrder.RepairTasks.First().Id);
    }
}
