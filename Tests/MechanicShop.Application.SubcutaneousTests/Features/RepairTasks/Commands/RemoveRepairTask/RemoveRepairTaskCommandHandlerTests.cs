using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.RemoveRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RemoveRepairTaskCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task RemoveRepairTaskHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var command = new DeleteRepairTaskCommand(repairTask.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var deletedRepairTask = await _context.RepairTasks.FirstOrDefaultAsync(r => r.Id == repairTask.Id);
        Assert.Null(deletedRepairTask);
    }

    [Fact]
    public async Task RemoveRepairTaskHandler_WithInvalidRepairTaskId_ShouldFail()
    {
        // Arrange
        var command = new DeleteRepairTaskCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundThisRepairTaskId.Code, result.TopError.Code);
    }

    [Fact]
    public async Task RemoveRepairTaskHandler_WithInUseRepairTask_ShouldFail()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        
        var make = VehicleMakeFactory.CreateVehicleMake().Value;
        var customer = MechanicShop.Tests.Common.Customers.CustomerFactory.CreateCustomer(email: "remove_rt@localhost.com", vehicles: [MechanicShop.Tests.Common.Customers.VehicleFactory.CreateVehicle(vehicleModelId: make.VehicleModels.First().Id).Value]).Value;
        var vehicle = customer.vehicles.First();
        
        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;

        await _context.VehicleMakes.AddAsync(make);
        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: employee.Id, repairTasks: [repairTask]).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new DeleteRepairTaskCommand(repairTask.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(RepairTaskErrors.InUse.Code, result.TopError.Code);
    }
}