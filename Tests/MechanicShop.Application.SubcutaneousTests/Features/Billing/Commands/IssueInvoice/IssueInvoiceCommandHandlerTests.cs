using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IssueInvoiceCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task IssueInvoiceHandler_WithValidCompletedWorkOrder_ShouldSucceed()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer().Value;
        customer.UpSertVehicles([vehicle]);

        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: employee.Id, repairTasks: [repairTask]).Value;
        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new IssueInvoiceCommand(workOrder.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
    }

    [Fact]
    public async Task IssueInvoiceHandler_WithNonExistentWorkOrder_ShouldFail()
    {
        // Arrange
        var command = new IssueInvoiceCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheWorkOrder.Code, result.TopError.Code);
    }

    [Fact]
    public async Task IssueInvoiceHandler_WithIncompleteWorkOrder_ShouldFail()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(email: "incomplete_wo@localhost.com").Value;
        customer.UpSertVehicles([vehicle]);

        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: employee.Id, repairTasks: [repairTask]).Value;
        // Keep it as Scheduled or InProgress
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new IssueInvoiceCommand(workOrder.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.WorkOrderMustBeCompletedToIssueInvoice.Code, result.TopError.Code);
    }

    [Fact]
    public async Task IssueInvoiceHandler_WhenInvoiceAlreadyIssued_ShouldFail()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(email: "already_issued@localhost.com").Value;
        customer.UpSertVehicles([vehicle]);

        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: employee.Id, repairTasks: [repairTask]).Value;
        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        // Issue once successfully
        var command = new IssueInvoiceCommand(workOrder.Id);
        await _mediator.Send(command);

        // Act - Issue second time
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.InvoiceAlreadyIssued.Code, result.TopError.Code);
    }
}
