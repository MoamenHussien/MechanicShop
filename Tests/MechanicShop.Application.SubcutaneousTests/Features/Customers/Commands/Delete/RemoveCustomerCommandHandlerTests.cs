using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.Delete;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RemoveCustomerCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task DeleteCustomerHandler_WithValidId_ShouldSucceed()
    {
        // Arrange
        var make = VehicleMakeFactory.CreateVehicleMake().Value;
        var customer = CustomerFactory.CreateCustomer(email: "delete1@localhost.com", vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: make.VehicleModels.First().Id).Value]).Value;

        await _context.VehicleMakes.AddAsync(make);
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new DeleteCustomerCommand(customer.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var deletedCustomer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.Null(deletedCustomer);
    }

    [Fact]
    public async Task DeleteCustomerHandler_WithInvalidId_ShouldFail()
    {
        // Arrange
        var command = new DeleteCustomerCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.TheCustomerNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task DeleteCustomerHandler_WhenCustomerHasWorkOrders_ShouldFail()
    {
        // Arrange
        var make = VehicleMakeFactory.CreateVehicleMake().Value;
        var customer = CustomerFactory.CreateCustomer(email: "delete2@localhost.com").Value;

        var vehicle = VehicleFactory.CreateVehicle(vehicleModelId: make.VehicleModels.First().Id).Value;
        customer.UpSertVehicles([vehicle]);

        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;
        var repairTask = MechanicShop.Tests.Common.RepaireTasks.RepairTaskFactory.CreateRepairTask().Value;

        await _context.VehicleMakes.AddAsync(make);
        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: employee.Id, repairTasks: [repairTask]).Value;
        await _context.WorkOrders.AddAsync(workOrder);

        await _context.SaveChangesAsync(default);

        var command = new DeleteCustomerCommand(customer.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.TheCustomerHasRecordForWorkOrderBefore.Code, result.TopError.Code);
    }
}
