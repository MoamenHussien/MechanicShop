
using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class CachingBehaviorTests
{
    private readonly HybridCache _cache = Substitute.For<HybridCache>();  // بيعمل Fake HybridCache
    private readonly ILogger<CachingBehavior<CachedQuery, Result<string>>> _logger = Substitute.For<ILogger<CachingBehavior<CachedQuery, Result<string>>>>();

    private readonly CachingBehavior<CachedQuery, Result<string>> _sut; //الكلاس اللى بنختبره  System Under Test

    public CachingBehaviorTests()
    {
        _sut = new CachingBehavior<CachedQuery, Result<string>>(_logger, _cache);
    }

    [Fact]
    public async Task Handle_WhenRequestDoesNotImplementICachedQuery_ShouldSkipCachingAndReturnHandlerResult()
    {
        // Arrange

        // Create a request that does NOT implement ICachedQuery to simulate a non-cacheable request.
        var uncachedRequest = new NonCachedQuery();
        // Create the real CachingBehavior with mocked dependencies.
        // The logger is irrelevant for this test, while the cache mock is used to verify that no caching operations occur.
        var behavior = new CachingBehavior<NonCachedQuery, string>(Substitute.For<ILogger<CachingBehavior<NonCachedQuery, string>>>(), _cache);

        // Act

        // Execute the behavior using a fake handler delegate that simply returns "OK".
        // This isolates the test to verify the behavior itself without invoking a real request handler.
        var result = await behavior.Handle(uncachedRequest, _ => Task.FromResult("OK"), CancellationToken.None); // act it return from real handler With value = OK

        // Assert

        // Verify that the behavior returns the exact result produced by the handler.
        Assert.Equal("OK", result);

        // Verify that nothing was stored in the cache because the request is not cacheable.
        await _cache.DidNotReceive().SetAsync(
            // Ignore the actual arguments; we only care that SetAsync was never called.
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRequestImplementICachedQueryAndResultIsSuccess_ShouldCacheResult() //AndResultIsSuccess → الـ Handler رجع Success
    {
        // Arrange

        // Create a cacheable request that implements ICachedQuery.
        var request = new CachedQuery();
        // Create a successful handler response that should be stored in the cache.
        var response = (Result<string>)"test-value";

        string? actualKey = null;
        object? actualValue = null;
        HybridCacheEntryOptions? actualOptions = null;
        string[]? actualTags = null;
        CancellationToken actualToken = default;
        // Variables used to capture the arguments passed to HybridCache.SetAsync.
        _cache.SetAsync(
            // Capture the cache key passed to SetAsync.
            Arg.Do<string>(k => actualKey = k),
            // Capture the value that will be stored in the cache.
            Arg.Do<object>(v => actualValue = v),
            // Capture the cache entry options (expiration, etc.).
            Arg.Do<HybridCacheEntryOptions>(o => actualOptions = o),
            // Capture the cache tags associated with the entry.
            Arg.Do<string[]>(t => actualTags = t),
            // Capture the cancellation token used for the cache operation.
            Arg.Do<CancellationToken>(c => actualToken = c)).Returns(ValueTask.CompletedTask);// Simulate a successful cache write operation.

        // Act

        // Execute the behavior with a successful handler response.
        var result = await _sut.Handle(request, _ => Task.FromResult(response), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(request.CacheKey, actualKey);

        var typed = Assert.IsType<Result<string>>(actualValue); // Verify that the cached object is of the expected type.
        Assert.True(typed.IsSuccess);
        Assert.Equal("test-value", typed.Value);

        Assert.Equal(request.Expiration, actualOptions!.Expiration);
        Assert.Equal(request.Tags, actualTags);
    }

    [Fact]
    public async Task Handle_WhenRequestImplementsICachedQueryAndResultIsError_ShouldNotCacheResult()
    {
        // Arrange

        // Create a cacheable request that implements ICachedQuery.
        var request = new CachedQuery();

        // Create a failed handler response to simulate an error scenario.
        var errorResult = (Result<string>)Error.Validation("code", "message");

        // Act

        // Execute the behavior using a handler that returns an error result.
        var result = await _sut.Handle(
            request,
            _ => Task.FromResult(errorResult),
            CancellationToken.None);

        // Assert

        // Verify that the behavior returns the same error produced by the handler.
        Assert.True(result.IsError);

        // Verify that failed results are never written to the cache.
        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(),
            Arg.Any<Result<string>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>());
    }

    public class NonCachedQuery;

    public class CachedQuery : ICachedQuery
    {
        public string CacheKey => "test-key";
        public TimeSpan Expiration => TimeSpan.FromMinutes(5);
        public string[] Tags => ["unit-test"];
    }
}
