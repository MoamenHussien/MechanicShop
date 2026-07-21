using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.RefreshTokens;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RefreshTokenQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task RefreshTokenHandler_WithValidTokens_ShouldSucceed()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var mediatorInScope = scope.ServiceProvider.GetRequiredService<IMediator>();

        var httpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext = httpContext;

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "refreshuser@localhost.com",
            UserName = "refreshuser@localhost.com",
            EmailConfirmed = true
        };
        var password = "ValidPassword123!";

        var createResult = await userManager.CreateAsync(appUser, password);
        Assert.True(createResult.Succeeded);

        // Generate initial tokens
        var generateCommand = new GenerateTokenCommand(appUser.Email!, password);
        var generateResult = await mediatorInScope.Send(generateCommand);
        Assert.True(generateResult.IsSuccess);

        var expiredAccessToken = generateResult.Value.AccessToken;

        // Move the RefreshToken cookie from the DB into the Request Headers 
        // to simulate the browser sending the cookie back
        var dbRefreshToken = await _context.RefreshTokens.FirstAsync(r => r.UserId == appUser.Id);
        httpContext.Request.Headers.Append("Cookie", $"RefreshToken={dbRefreshToken.Token}");

        var refreshCommand = new RefreshTokenCommand(expiredAccessToken!);

        // Act
        var result = await mediatorInScope.Send(refreshCommand);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.True(result.Value.ExpiresOnUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshTokenHandler_WithMissingCookie_ShouldFail()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var mediatorInScope = scope.ServiceProvider.GetRequiredService<IMediator>();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "missingcookieuser@localhost.com",
            UserName = "missingcookieuser@localhost.com",
            EmailConfirmed = true
        };
        var password = "ValidPassword123!";

        var createResult = await userManager.CreateAsync(appUser, password);
        Assert.True(createResult.Succeeded);

        // Generate a real, validly-signed JWT token
        var generateCommand = new GenerateTokenCommand(appUser.Email!, password);
        var generateResult = await mediatorInScope.Send(generateCommand);
        Assert.True(generateResult.IsSuccess);

        var realAccessToken = generateResult.Value.AccessToken;

        // Reset the HttpContext so the cookie generated above is erased
        var httpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext = httpContext; // No cookies attached

        var command = new RefreshTokenCommand(realAccessToken!);

        // Act
        var result = await mediatorInScope.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Auth.RefreshToken.Missing", result.TopError.Code);
    }
}