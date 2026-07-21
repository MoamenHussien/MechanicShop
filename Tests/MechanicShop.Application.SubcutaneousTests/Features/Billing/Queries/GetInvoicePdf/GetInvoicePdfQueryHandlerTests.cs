using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePdf;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetInvoicePdfQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetInvoicePdfHandler_WithValidInvoice_ShouldSucceed()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(email: "pdf1@localhost.com").Value;
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

        var query = new GetInvoicePdfQuery(invoiceId);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.Content);
        Assert.NotEmpty(result.Value.Content);
        Assert.Equal($"Invoice-{invoiceId}.Pdf", result.Value.FileName);
    }

    [Fact]
    public async Task GetInvoicePdfHandler_WithInvalidInvoice_ShouldFail()
    {
        // Arrange
        var query = new GetInvoicePdfQuery(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
    }
}