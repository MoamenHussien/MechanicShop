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

       var Data = await cache.GetOrCreateAsync<TResponse>(
                                                          cachedQuery.CacheKey,
                                                          _=> new ValueTask<TResponse>((TResponse)(object)null!),
                                                           new HybridCacheEntryOptions
                                                           {
                                                               Flags = HybridCacheEntryFlags.DisableUnderlyingData
                                                           },
                                                           cancellationToken:cancellationToken) ;
        

       if (Data is null)
        {
            logger.LogInformation("Cache miss for {RequestName} with key {CacheKey}", typeof(TRequest).Name, cachedQuery.CacheKey);
            Data = await next(cancellationToken);

            if (Data is IResult result && result.IsSuccess)
            {
               await cache.SetAsync(cachedQuery.CacheKey,
                Data,
                new HybridCacheEntryOptions
                {
                    Expiration = cachedQuery.Expiration
                },
                cachedQuery.Tags,
                cancellationToken
                );
                 logger.LogInformation("Data cached for request {RequestName} with key {CacheKey}", typeof(TRequest).Name, cachedQuery.CacheKey);
            }
        }
        return Data;
    }
}