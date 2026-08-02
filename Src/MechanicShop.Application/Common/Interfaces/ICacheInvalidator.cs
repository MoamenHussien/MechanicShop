namespace MechanicShop.Application.Common.Interfaces;

public interface ICacheInvalidator
{
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default);

    Task EvictByTagsAsync(params string[] tags);

    Task EvictByTagsAsync(CancellationToken cancellationToken, params string[] tags);
}
