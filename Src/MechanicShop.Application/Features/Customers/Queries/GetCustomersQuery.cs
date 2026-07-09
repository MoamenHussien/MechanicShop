using System.Runtime.InteropServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record GetCustomersQuery() : ICachedQuery<Result<List<CustomerDto>>>
{
    public string CacheKey => "AllCustomers";

    public string[] Tags => ["Customers"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetCustomersQueryHandler(ILogger<GetCustomersQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetCustomersQuery,Result<List<CustomerDto>>>
{
    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await context.Customers.Include(n=>n.vehicles).AsNoTracking().Select(n=>n.ToDto()).ToListAsync(cancellationToken);
        if (!customers.Any())
        {
          logger.LogWarning("Not Found Any Of Customers");
           return ApplicationErrors.NotFoundAnyCustomers;
        }

        logger.LogInformation("Cache miss, returning all customers With Count: {Count}", customers.Count);
        return customers;
    }
}