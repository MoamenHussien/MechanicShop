using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace MechanicShop.Api.OutputCaching;

public sealed class AuthenticatedRequestCachingPolicy(
    ILogger<AuthenticatedRequestCachingPolicy> logger) : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var canCache = CanCacheRequest(context);
        var path = context.HttpContext.Request.Path;

        logger.LogInformation(
            "🔍 OutputCache CHECK - Evaluating request '{Path}'. Eligible for caching: {CanCache}",
            path,
            canCache);

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = canCache;
        context.AllowCacheStorage = canCache;
        context.AllowLocking = true;

        // Vary by all query string parameters.
        context.CacheVaryByRules.QueryKeys = "*";

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "🚀 OutputCache HIT - Serving cached response for '{Path}'.",
            context.HttpContext.Request.Path);

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;
        var path = context.HttpContext.Request.Path;

        // Never cache responses that issue cookies.
        if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
        {
            logger.LogWarning(
                "❌ OutputCache SKIP - Response contains 'Set-Cookie'. Caching skipped for '{Path}'.",
                path);

            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        // Cache only successful responses.
        if (response.StatusCode != StatusCodes.Status200OK)
        {
            logger.LogWarning(
                "❌ OutputCache SKIP - Response status code {StatusCode} is not cacheable for '{Path}'.",
                response.StatusCode,
                path);

            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        // This custom policy explicitly allows caching authenticated responses.
        // The framework marks cache storage as disabled before this callback for
        // authenticated requests, so we re-enable it after applying our own safety checks.
        context.AllowCacheStorage = true;

        logger.LogInformation(
            "💾 OutputCache STORE - Response cached successfully for '{Path}'.",
            path);

        return ValueTask.CompletedTask;
    }

    private static bool CanCacheRequest(OutputCacheContext context)
    {
        var request = context.HttpContext.Request;

        // Cache GET and HEAD requests only.
        return HttpMethods.IsGet(request.Method) ||
               HttpMethods.IsHead(request.Method);
    }
}
