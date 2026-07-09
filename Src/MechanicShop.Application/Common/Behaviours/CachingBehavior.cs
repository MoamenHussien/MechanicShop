using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public class CachingBehavior<TRequest, TResponse>
(ILogger<CachingBehavior<TRequest, TResponse>> logger,
 HybridCache cache) : IPipelineBehavior<TRequest, TResponse>
 where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedQuery)
        {
            return await next(cancellationToken);
        }
        
        logger.LogInformation(
            "🔍 HybridCache CHECK - Evaluating request '{RequestName}' with key '{CacheKey}'.",
            typeof(TRequest).Name,
            cachedQuery.CacheKey);

        var data = await cache.GetOrCreateAsync<TResponse>(
                                                           cachedQuery.CacheKey,
                                                           _ => new ValueTask<TResponse>((TResponse)(object)null!),
                                                            new HybridCacheEntryOptions
                                                            {
                                                                Flags = HybridCacheEntryFlags.DisableUnderlyingData
                                                            },
                                                            cancellationToken: cancellationToken);
        if (data is not null)
        {
            logger.LogInformation(
                "🚀 HybridCache HIT - Serving cached data for '{RequestName}' with key '{CacheKey}'.",
                typeof(TRequest).Name,
                cachedQuery.CacheKey);

            return data;
        }

        logger.LogInformation(
           "❌ HybridCache MISS - Cache entry not found for '{RequestName}' with key '{CacheKey}'.",
           typeof(TRequest).Name,
           cachedQuery.CacheKey);

        data = await next(cancellationToken);

        if (data is IResult result && result.IsSuccess)
        {
            await cache.SetAsync(
                cachedQuery.CacheKey,
                data,
                new HybridCacheEntryOptions
                {
                    Expiration = cachedQuery.Expiration
                },
                cachedQuery.Tags,
                cancellationToken);

            logger.LogInformation(
                "💾 HybridCache STORE - Response cached successfully for '{RequestName}' with key '{CacheKey}'.",
                typeof(TRequest).Name,
                cachedQuery.CacheKey);
        }

        return data;
    }
}