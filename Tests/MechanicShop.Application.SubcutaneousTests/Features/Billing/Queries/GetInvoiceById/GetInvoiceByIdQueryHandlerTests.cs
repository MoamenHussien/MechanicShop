using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoiceById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetInvoiceByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetInvoiceByIdHandler_WithValidId_ShouldSucceed()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(email: "getinvoice1@localhost.com").Value;
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

        var query = new GetInvoiceByIdQuery(invoiceId);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(invoiceId, result.Value.InvoiceId);
    }

    [Fact]
    public async Task GetInvoiceByIdHandler_WithInvalidId_ShouldFail()
    {
        // Arrange
        var query = new GetInvoiceByIdQuery(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
    }
}
