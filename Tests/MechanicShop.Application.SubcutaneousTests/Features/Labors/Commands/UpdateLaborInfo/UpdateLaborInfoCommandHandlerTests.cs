using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Commands.UpdateLaborInfo;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateLaborInfoCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task UpdateLaborInfoHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>();

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "labor_info@test.com",
            UserName = "labor_info@test.com",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(appUser, "Password123!");

        var employee = EmployeeFactory.CreateEmployee(id: appUser.Id).Value;

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var command = new UpdateLaborInfoCommand(
            id: employee.Id,
            FirstName: "UpdatedFirstName",
            LastName: "UpdatedLastName",
            IsActive: false);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var updatedEmployee = await _context.Employees
            .AsNoTracking()
            .SingleAsync(e => e.Id == employee.Id);

        Assert.Equal(command.FirstName.CapitalizeFirstLetter(), updatedEmployee.FirstName);
        Assert.Equal(command.LastName.CapitalizeFirstLetter(), updatedEmployee.LastName);
        Assert.Equal(command.IsActive, updatedEmployee.IsActive);
    }

    [Fact]
    public async Task UpdateLaborInfoHandler_WithInvalidLaborId_ShouldFail()
    {
        // Arrange
        var command = new UpdateLaborInfoCommand(
            id: Guid.NewGuid(),
            FirstName: "UpdatedFirstName",
            LastName: "UpdatedLastName",
            IsActive: true);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundTheLabor.Code, result.TopError.Code);
    }
}
