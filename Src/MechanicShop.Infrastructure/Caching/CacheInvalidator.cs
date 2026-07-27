using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Caching;

public sealed class CacheInvalidator(
    HybridCache hybridCache,
    IOutputCacheStore outputCacheStore,
    ILogger<CacheInvalidator> logger) : ICacheInvalidator
{
    public async Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;

        logger.LogInformation("🗑️  HybridCache & OutputCache EVICT - Cache tag '{Tag}' evicted successfully.",tag);

        await hybridCache.RemoveByTagAsync(tag, cancellationToken);
        await outputCacheStore.EvictByTagAsync(tag, cancellationToken);
    }

    public async Task EvictByTagsAsync(params string[] tags)
    {
        await EvictByTagsAsync(CancellationToken.None, tags);
    }

    public async Task EvictByTagsAsync(CancellationToken cancellationToken, params string[] tags)
    {
        if (tags is null || tags.Length == 0) return;

        foreach (var tag in tags)
        {
            await EvictByTagAsync(tag, cancellationToken);
        }
    }
}
