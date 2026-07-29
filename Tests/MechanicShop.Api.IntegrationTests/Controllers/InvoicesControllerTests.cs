using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class InvoicesControllerTests
{
    private readonly WebAppFactory _webAppFactory;
    private readonly AppHttpClient _client;
    private readonly IAppDbContext _context;

    public InvoicesControllerTests(WebAppFactory webAppFactory)
    {
        _webAppFactory = webAppFactory;
        _client = webAppFactory.CreateAppHttpClient();
        _context = webAppFactory.CreateAppDbContext();
    }


    // ========================================================================
    // POST /api/v{version}/invoices/workorders/{workOrderId} (IssueInvoice)
    // ========================================================================

    [Fact]
    public async Task IssueInvoice_WithValidCompletedWorkOrder_ShouldReturnCreated()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .Completed()
            .WithRepairTasks(await _context.RepairTasks.Include(r => r.Parts).Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>();
            Assert.NotNull(invoice);
            Assert.Equal(workOrder.Id, invoice!.WorkOrderId);
        }
        finally
        {
            await _context.Invoices.Where(i => i.WorkOrderId == workOrder.Id).ExecuteDeleteAsync();
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task IssueInvoice_WithIncompleteWorkOrder_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .ForToday() // Defaults to Scheduled or something not Completed
            .WithRepairTasks(await _context.RepairTasks.Include(r => r.Parts).Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // WorkOrderMustBeCompletedToIssueInvoice -> 400
        }
        finally
        {
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task IssueInvoice_WithNonExistentWorkOrder_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentWorkOrderId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{nonExistentWorkOrderId}", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task IssueInvoice_WhenAlreadyIssued_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .Completed()
            .WithRepairTasks(await _context.RepairTasks.Include(r => r.Parts).Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        try
        {
            // Issue first time
            var firstResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });
            firstResponse.EnsureSuccessStatusCode();

            // Attempt to issue again
            var secondResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });

            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode); // InvoiceAlreadyIssued -> 409
        }
        finally
        {
            await _context.Invoices.Where(i => i.WorkOrderId == workOrder.Id).ExecuteDeleteAsync();
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task IssueInvoice_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01); // Labor has no Manager role
        _client.SetAuthorizationHeader(token);

        var workOrderId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrderId}", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // GET /api/v{version}/invoices/{invoiceId} (GetInvoice)
    // ========================================================================

    [Fact]
    public async Task GetInvoice_WithValidId_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .Completed()
            .WithRepairTasks(await _context.RepairTasks.Include(r => r.Parts).Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        try
        {
            var issueResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });
            var issuedInvoice = await issueResponse.Content.ReadFromJsonAsync<InvoiceDto>();

            var getResponse = await _client.GetAsync($"/api/v1.0/invoices/{issuedInvoice!.InvoiceId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetchedInvoice = await getResponse.Content.ReadFromJsonAsync<InvoiceDto>();

            Assert.NotNull(fetchedInvoice);
            Assert.Equal(issuedInvoice.InvoiceId, fetchedInvoice!.InvoiceId);
        }
        finally
        {
            await _context.Invoices.Where(i => i.WorkOrderId == workOrder.Id).ExecuteDeleteAsync();
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetInvoice_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/invoices/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoice_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var invoiceId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/invoices/{invoiceId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v{version}/invoices/{invoiceId}/payments (SettleInvoice)
    // ========================================================================

    [Fact]
    public async Task SettleInvoice_WithValidId_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .Completed()
            .WithRepairTasks(await _context.RepairTasks.Include(r => r.Parts).Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        try
        {
            var issueResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });
            var issuedInvoice = await issueResponse.Content.ReadFromJsonAsync<InvoiceDto>();

            var settleResponse = await _client.PutAsJsonAsync($"/api/v1.0/invoices/{issuedInvoice!.InvoiceId}/payments", new { });

            Assert.Equal(HttpStatusCode.NoContent, settleResponse.StatusCode);
        }
        finally
        {
            await _context.Invoices.Where(i => i.WorkOrderId == workOrder.Id).ExecuteDeleteAsync();
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task SettleInvoice_WhenAlreadyPaid_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .Completed()
            .WithRepairTasks(await _context.RepairTasks.Include(r => r.Parts).Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        try
        {
            var issueResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });
            var issuedInvoice = await issueResponse.Content.ReadFromJsonAsync<InvoiceDto>();

            var firstSettleResponse = await _client.PutAsJsonAsync($"/api/v1.0/invoices/{issuedInvoice!.InvoiceId}/payments", new { });
            firstSettleResponse.EnsureSuccessStatusCode();

            var secondSettleResponse = await _client.PutAsJsonAsync($"/api/v1.0/invoices/{issuedInvoice.InvoiceId}/payments", new { });

            Assert.Equal(HttpStatusCode.Conflict, secondSettleResponse.StatusCode);
        }
        finally
        {
            await _context.Invoices.Where(i => i.WorkOrderId == workOrder.Id).ExecuteDeleteAsync();
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task SettleInvoice_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync($"/api/v1.0/invoices/{nonExistentId}/payments", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ========================================================================
    // GET /api/v{version}/invoices/{invoiceId}/pdf (GetInvoicePdf)
    // ========================================================================

    [Fact]
    public async Task GetInvoicePdf_WithValidId_ShouldReturnFile()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .Completed()
            .WithRepairTasks(await _context.RepairTasks.Include(r => r.Parts).Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        try
        {
            var issueResponse = await _client.PostAsJsonAsync($"/api/v1.0/invoices/workorders/{workOrder.Id}", new { });
            var issuedInvoice = await issueResponse.Content.ReadFromJsonAsync<InvoiceDto>();

            var pdfResponse = await _client.GetAsync($"/api/v1.0/invoices/{issuedInvoice!.InvoiceId}/pdf");

            Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
            Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType?.MediaType);
        }
        finally
        {
            await _context.Invoices.Where(i => i.WorkOrderId == workOrder.Id).ExecuteDeleteAsync();
            await _context.WorkOrders.Where(w => w.Id == workOrder.Id).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetInvoicePdf_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/invoices/{nonExistentId}/pdf");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
