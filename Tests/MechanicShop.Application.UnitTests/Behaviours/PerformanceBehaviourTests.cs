using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class PerformanceBehaviourTests
{
    private readonly ILogger<TestRequest> _loggerMock;
    private readonly IUser _currentUserMock;
    private readonly IIdentityService _identityServiceMock;
    private readonly PerformanceBehavior<TestRequest, TestResponse> _sut;

    public PerformanceBehaviourTests()
    {
        _loggerMock = Substitute.For<ILogger<TestRequest>>();
        _currentUserMock = Substitute.For<IUser>();
        _identityServiceMock = Substitute.For<IIdentityService>();

        _sut = new PerformanceBehavior<TestRequest, TestResponse>(
            _loggerMock,
            _currentUserMock,
            _identityServiceMock);
    }

    [Fact]
    public async Task Handle_WhenRequestCompletesWithinThreshold_ShouldNotLogWarning()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };

        // Act

        // Execute a fast request that completes before the warning threshold.
        var result = await _sut.Handle(
            request,
            _ => Task.FromResult(expectedResponse),
            CancellationToken.None);

        // Assert

        // Verify that the original handler response is returned.
        Assert.Equal(expectedResponse, result);

        // Verify that no warning log is written for fast requests.
        _loggerMock.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenRequestExceedsThreshold_ShouldLogWarningWithUserInformation()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };

        var currentUserId = Guid.NewGuid();
        const string currentUserName = "TestUser";

        // Simulate an authenticated user.
        _currentUserMock.Id.Returns(currentUserId);

        // Configure the identity service to return the user's display name.
        _identityServiceMock
            .GetUserNameAsync(currentUserId)
            .Returns(currentUserName);

        // Act

        // Execute a slow request that exceeds the performance threshold.
        var result = await _sut.Handle(
            request,
            async _ =>
            {
                await Task.Delay(600);
                return expectedResponse;
            },
            CancellationToken.None);

        // Assert

        // Verify that the original handler response is returned.
        Assert.Equal(expectedResponse, result);

        // Verify that a warning log was written containing the request and user information.
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state =>
                state!.ToString()!.Contains("Long running request") &&
                state.ToString()!.Contains(nameof(TestRequest)) &&
                state.ToString()!.Contains(currentUserId.ToString()) &&
                state.ToString()!.Contains(currentUserName)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldLogWarningWithoutResolvingUserName()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };

        Guid? currentUserId = null;

        // Simulate an anonymous request.
        _currentUserMock.Id.Returns(currentUserId);

        // Act

        // Execute a slow request.
        var result = await _sut.Handle(
            request,
            async _ =>
            {
                await Task.Delay(600);
                return expectedResponse;
            },
            CancellationToken.None);

        // Assert

        // Verify that the original handler response is returned.
        Assert.Equal(expectedResponse, result);

        // Verify that a warning log was still written.
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state =>
                state!.ToString()!.Contains("Long running request") &&
                state.ToString()!.Contains(nameof(TestRequest))),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        // Verify that no username lookup was performed because no user id exists.
        await _identityServiceMock
            .DidNotReceive()
            .GetUserNameAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WhenUserIdIsGuidEmpty_ShouldLogWarningWithoutResolvingUserName()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };

        // Simulate an empty user identifier.
        _currentUserMock.Id.Returns(Guid.Empty);

        // Act

        // Execute a slow request.
        var result = await _sut.Handle(
            request,
            async _ =>
            {
                await Task.Delay(600);
                return expectedResponse;
            },
            CancellationToken.None);

        // Assert

        // Verify that the original handler response is returned.
        Assert.Equal(expectedResponse, result);

        // Verify that a warning log was still written.
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state =>
                state!.ToString()!.Contains("Long running request") &&
                state.ToString()!.Contains(nameof(TestRequest))),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        // Verify that no username lookup was performed for an empty user id.
        await _identityServiceMock
            .DidNotReceive()
            .GetUserNameAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceReturnsNull_ShouldLogWarningWithEmptyUserName()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };

        var currentUserId = Guid.NewGuid();

        // Simulate an authenticated user.
        _currentUserMock.Id.Returns(currentUserId);

        // Simulate a missing username.
        _identityServiceMock
            .GetUserNameAsync(currentUserId)
            .Returns((string?)null);

        // Act

        // Execute a slow request.
        var result = await _sut.Handle(
            request,
            async _ =>
            {
                await Task.Delay(600);
                return expectedResponse;
            },
            CancellationToken.None);

        // Assert

        // Verify that the original handler response is returned.
        Assert.Equal(expectedResponse, result);

        // Verify that a warning log was still written even when the username is unavailable.
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ShouldAlwaysReturnHandlerResponse()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Result = "Success" };

        // Act

        // Execute the behavior.
        var result = await _sut.Handle(
            request,
            _ => Task.FromResult(expectedResponse),
            CancellationToken.None);

        // Assert

        // Verify that the behavior always returns the handler response.
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert

        // Verify that the behavior does not swallow exceptions thrown by the handler.
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Handle(
                request,
                _ => throw expectedException,
                CancellationToken.None));

        Assert.Equal(expectedException, actualException);
    }

    public class TestRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}
