using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class UnhandledExceptionBehaviourTests
{
    private readonly ILogger<DummyRequest> _logger = Substitute.For<ILogger<DummyRequest>>();
    private readonly UnhandledExceptionBehavior<DummyRequest, string> _sut;

    public UnhandledExceptionBehaviourTests()
    {
        _sut = new UnhandledExceptionBehavior<DummyRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_WhenNoException_InvokesNextAndReturnsResult()
    {
        // Arrange
        var request = new DummyRequest();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("OK");

        // Act
        var result = await _sut.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal("OK", result);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_LogsErrorAndRethrows()
    {
        // Arrange
        var request = new DummyRequest();
        var exception = new InvalidOperationException("test failure");

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns<Task<string>>(_ => throw exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(request, next, CancellationToken.None));

        Assert.Equal(exception, ex);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state != null && state.ToString()!.Contains("UnHandle Exception")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    public class DummyRequest;
}
