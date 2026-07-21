using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CustomersControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    // ========================================================================
    // GET /api/v1.0/customers
    // ========================================================================

    [Fact]
    public async Task GetCustomers_WithValidRequest_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCustomers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // GET /api/v1.0/customers/{customerId}
    // ========================================================================

    [Fact]
    public async Task GetCustomerById_WithValidId_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var customer = await _context.Customers.FirstOrDefaultAsync();

        Assert.NotNull(customer);

        var response = await _client.GetAsync($"/api/v1.0/customers/{customer!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result!.CustomerId);
    }

    [Fact]
    public async Task GetCustomerById_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/customers/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var customerId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/customers/{customerId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // POST /api/v1.0/customers
    // ========================================================================

    [Fact]
    public async Task CreateCustomer_WithValidRequest_ShouldReturnCreated()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var vehicleModel = await _context.VehicleModels.FirstAsync();

        var request = new CreateCustomerRequest
        {
            Name = "Integration Test Customer",
            Email = $"inttest-{Guid.NewGuid():N}@localhost",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = vehicleModel.Id,
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        CustomerDto? dto = null;
        try
        {
            var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            dto = await response.Content.ReadFromJsonAsync<CustomerDto>();

            Assert.NotNull(dto);
            Assert.Equal(request.Name, dto!.Name);
            Assert.Equal(request.Email.Trim().ToLowerInvariant(), dto.Email);
            Assert.Equal(request.PhoneNumber, dto.PhoneNumber);
            Assert.Single(dto.Vehicles);
        }
        finally
        {
            if (dto is not null)
            {
                await _context.Customers
                    .Where(c => c.Id == dto.CustomerId)
                    .ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerRequest
        {
            Name = "",
            Email = "",
            PhoneNumber = "",
            Vehicles = []
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = "not-an-email",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = Guid.NewGuid(),
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidPhone_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = "valid@localhost",
            PhoneNumber = "abc",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = Guid.NewGuid(),
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithEmptyVehicles_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = "test@localhost",
            PhoneNumber = "+201012345678",
            Vehicles = []
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var existingCustomer = await _context.Customers.FirstAsync();

        var vehicleModel = await _context.VehicleModels.FirstAsync();

        var request = new CreateCustomerRequest
        {
            Name = "Duplicate Email Customer",
            Email = existingCustomer.Email,
            PhoneNumber = "+201099999999",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = vehicleModel.Id,
                    Year = 2024,
                    LicensePlate = "DUP123"
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithNonExistentVehicleModel_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = $"modeltest-{Guid.NewGuid():N}@localhost",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = Guid.NewGuid(),
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = "test@localhost",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = Guid.NewGuid(),
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = "test@localhost",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = Guid.NewGuid(),
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/customers", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v1.0/customers/{customerId}
    // ========================================================================

    [Fact]
    public async Task UpdateCustomer_WithValidRequest_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var vehicleModel = await _context.VehicleModels.FirstAsync();

        // Create a customer to update
        var createRequest = new CreateCustomerRequest
        {
            Name = "Update Test Customer",
            Email = $"update-{Guid.NewGuid():N}@localhost",
            PhoneNumber = "+201055555555",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = vehicleModel.Id,
                    Year = 2023,
                    LicensePlate = "UPD123"
                }
            ]
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1.0/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(createdCustomer);

        try
        {
            var updateRequest = new UpdateCustomerRequest
            {
                Name = "Updated Customer Name",
                Email = createRequest.Email,
                PhoneNumber = "+201066666666",
                Vehicles =
                [
                    new UpdateVehicleRequest
                    {
                        VehicleId = createdCustomer!.Vehicles.First().Id,
                        ModelId = vehicleModel.Id,
                        Year = 2025,
                        LicensePlate = "UPD456"
                    }
                ]
            };

            var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{createdCustomer.CustomerId}", updateRequest);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.Customers
                .Where(c => c.Id == createdCustomer!.CustomerId)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var vehicleModel = await _context.VehicleModels.FirstAsync();

        var nonExistentId = Guid.NewGuid();

        var request = new UpdateCustomerRequest
        {
            Name = "Does Not Exist",
            Email = "noexist@localhost",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new UpdateVehicleRequest
                {
                    ModelId = vehicleModel.Id,
                    Year = 2024,
                    LicensePlate = "NXS123"
                }
            ]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{nonExistentId}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new UpdateCustomerRequest
        {
            Name = "",
            Email = "",
            PhoneNumber = "",
            Vehicles = []
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_WithDuplicateEmail_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var vehicleModel = await _context.VehicleModels.FirstAsync();

        // Create two customers
        var email1 = $"dup1-{Guid.NewGuid():N}@localhost";
        var email2 = $"dup2-{Guid.NewGuid():N}@localhost";

        var createRequest1 = new CreateCustomerRequest
        {
            Name = "Customer One",
            Email = email1,
            PhoneNumber = "+201011111111",
            Vehicles = [new CreateVehicleRequest { ModelId = vehicleModel.Id, Year = 2024, LicensePlate = "DU1123" }]
        };

        var createRequest2 = new CreateCustomerRequest
        {
            Name = "Customer Two",
            Email = email2,
            PhoneNumber = "+201022222222",
            Vehicles = [new CreateVehicleRequest { ModelId = vehicleModel.Id, Year = 2024, LicensePlate = "DU2123" }]
        };

        var response1 = await _client.PostAsJsonAsync("/api/v1.0/customers", createRequest1);
        var customer1 = await response1.Content.ReadFromJsonAsync<CustomerDto>();

        var response2 = await _client.PostAsJsonAsync("/api/v1.0/customers", createRequest2);
        var customer2 = await response2.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(customer1);
        Assert.NotNull(customer2);

        try
        {
            // Try to update customer2's email to customer1's email
            var updateRequest = new UpdateCustomerRequest
            {
                Name = "Customer Two Updated",
                Email = email1,
                PhoneNumber = "+201022222222",
                Vehicles = [new UpdateVehicleRequest
                {
                    VehicleId = customer2!.Vehicles.First().Id,
                    ModelId = vehicleModel.Id,
                    Year = 2024,
                    LicensePlate = "DU2123"
                }]
            };

            var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{customer2.CustomerId}", updateRequest);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            await _context.Customers
                .Where(c => c.Id == customer1!.CustomerId || c.Id == customer2!.CustomerId)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateCustomer_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var request = new UpdateCustomerRequest
        {
            Name = "Test",
            Email = "test@localhost",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new UpdateVehicleRequest
                {
                    ModelId = Guid.NewGuid(),
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new UpdateCustomerRequest
        {
            Name = "Test",
            Email = "test@localhost",
            PhoneNumber = "+201012345678",
            Vehicles =
            [
                new UpdateVehicleRequest
                {
                    ModelId = Guid.NewGuid(),
                    Year = 2024,
                    LicensePlate = "TST123"
                }
            ]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/customers/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // DELETE /api/v1.0/customers/{customerId}
    // ========================================================================

    [Fact]
    public async Task DeleteCustomer_WithValidId_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var vehicleModel = await _context.VehicleModels.FirstAsync();

        // Create a customer to delete
        var createRequest = new CreateCustomerRequest
        {
            Name = "Delete Test Customer",
            Email = $"delete-{Guid.NewGuid():N}@localhost",
            PhoneNumber = "+201077777777",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = vehicleModel.Id,
                    Year = 2024,
                    LicensePlate = "DEL123"
                }
            ]
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1.0/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(createdCustomer);

        var response = await _client.DeleteAsync($"/api/v1.0/customers/{createdCustomer!.CustomerId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the customer was actually deleted
        var getResponse = await _client.GetAsync($"/api/v1.0/customers/{createdCustomer.CustomerId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/customers/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_WithActiveWorkOrders_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var vehicleModel = await _context.VehicleModels.FirstAsync();

        // Create a customer
        var createRequest = new CreateCustomerRequest
        {
            Name = "Conflict Test Customer",
            Email = $"conflict-{Guid.NewGuid():N}@localhost",
            PhoneNumber = "+201088888888",
            Vehicles =
            [
                new CreateVehicleRequest
                {
                    ModelId = vehicleModel.Id,
                    Year = 2024,
                    LicensePlate = "CNF123"
                }
            ]
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1.0/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(createdCustomer);

        // Create a work order for this customer's vehicle
        var vehicle = createdCustomer!.Vehicles.First();
        var repairTaskIds = _context.RepairTasks.Select(rt => rt.Id).Take(1).ToList();

        var workOrder = WorkOrderTestDataBuilder.Create()
            .ForToday()
            .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
            .WithVehicle(vehicle.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.DeleteAsync($"/api/v1.0/customers/{createdCustomer.CustomerId}");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                .Where(w => w.Id == workOrder.Id)
                .ExecuteDeleteAsync();

            await _context.Customers
                .Where(c => c.Id == createdCustomer.CustomerId)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task DeleteCustomer_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var customerId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/customers/{customerId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.DeleteAsync($"/api/v1.0/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
