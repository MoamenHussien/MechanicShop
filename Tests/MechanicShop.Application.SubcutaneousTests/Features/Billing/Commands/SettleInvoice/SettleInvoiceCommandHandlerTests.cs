using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettleInvoiceCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task SettleInvoiceHandler_WithValidInvoice_ShouldSucceed()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(email: "settle1@localhost.com").Value;
        customer.UpSertVehicles([vehicle]);
        
        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;
        var repairTask = MechanicShop.Tests.Common.RepaireTasks.RepairTaskFactory.CreateRepairTask().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: employee.Id, repairTasks: [repairTask]).Value;
        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var issueCommand = new IssueInvoiceCommand(workOrder.Id);
        var issueResult = await _mediator.Send(issueCommand);
        Assert.True(issueResult.IsSuccess);
        
        var invoiceId = issueResult.Value.InvoiceId;

        var command = new SettleInvoiceCommand(invoiceId);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        
        var settledInvoice = await _context.Invoices.FindAsync(invoiceId);
        Assert.NotNull(settledInvoice);
        Assert.Equal(InvoiceStatus.Paid, settledInvoice.Status);
    }

    [Fact]
    public async Task SettleInvoiceHandler_WithInvalidInvoiceId_ShouldFail()
    {
        // Arrange
        var command = new SettleInvoiceCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task SettleInvoiceHandler_WhenInvoiceIsAlreadyPaid_ShouldFail()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(email: "settle2@localhost.com").Value;
        customer.UpSertVehicles([vehicle]);
        
        var employee = MechanicShop.Tests.Common.Employees.EmployeeFactory.CreateEmployee().Value;
        var repairTask = MechanicShop.Tests.Common.RepaireTasks.RepairTaskFactory.CreateRepairTask().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Employees.AddAsync(employee);
        await _context.RepairTasks.AddAsync(repairTask);

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: employee.Id, repairTasks: [repairTask]).Value;
        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var issueCommand = new IssueInvoiceCommand(workOrder.Id);
        var issueResult = await _mediator.Send(issueCommand);
        Assert.True(issueResult.IsSuccess);
        
        var invoiceId = issueResult.Value.InvoiceId;

        // Pay the first time
        var command = new SettleInvoiceCommand(invoiceId);
        await _mediator.Send(command);

        // Act - Pay second time
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.InvoiceIsAlreadyPaid.Code, result.TopError.Code);
    }
}