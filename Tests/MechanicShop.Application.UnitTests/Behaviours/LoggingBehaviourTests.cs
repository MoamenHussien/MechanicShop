using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class LoggingBehaviourTests
{
    private readonly ILogger<DummyRequest> _loggerMock = Substitute.For<ILogger<DummyRequest>>();
    private readonly IUser _currentUserMock = Substitute.For<IUser>();
    private readonly IIdentityService _identityServiceMock = Substitute.For<IIdentityService>();

    private readonly LoggingBehavior<DummyRequest> _sut;

    public LoggingBehaviourTests()
    {
        _sut = new LoggingBehavior<DummyRequest>(
            _loggerMock,
            _currentUserMock,
            _identityServiceMock);
    }

    [Fact]
    public async Task Process_WhenUserIdExists_ShouldResolveUserNameAndLogRequest()
    {
        // Arrange

        var dummyRequest = new DummyRequest();
        var currentUserId = Guid.NewGuid();

        // Simulate an authenticated user.
        _currentUserMock.Id.Returns(currentUserId);

        // Configure the identity service to return the user's display name.
        _identityServiceMock
            .GetUserNameAsync(currentUserId)
            .Returns("Issam");

        // Act

        // Execute the logging behavior.
        await _sut.Process(dummyRequest, CancellationToken.None);

        // Assert

        // Verify that the user's name was resolved from the identity service.
        await _identityServiceMock
            .Received(1)   //اتأكد إن الميثود دى اتنادت مرة واحدة
            .GetUserNameAsync(currentUserId);   // GetUserNameAsync(currentUserId) هل فعلاً الـ LoggingBehavior عمل
 
        // Verify that an information log entry was written for the processed request.

        _loggerMock.Received(1).Log(  //اتأكد إن الـ Logger عمل Log مرة واحدة
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state!.ToString()!.Contains("Request")), //هات الرسالة اللى اتكتبت حولها String. هل فيها كلمة Request
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Process_WhenUserIdIsMissing_ShouldSkipUserLookupAndLogRequest()
    {
        // Arrange

        var dummyRequest = new DummyRequest();
        Guid? currentUserId = null;

        // Simulate an anonymous request with no authenticated user.
        _currentUserMock.Id.Returns(currentUserId);

        // Act

        // Execute the logging behavior.
        await _sut.Process(dummyRequest, CancellationToken.None);

        // Assert

        // Verify that no user lookup was performed because no user id exists.
        await _identityServiceMock
            .DidNotReceive()
            .GetUserNameAsync(Arg.Any<Guid>());

        // Verify that the request was still logged successfully.
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state!.ToString()!.Contains("Request")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    public class DummyRequest;
}