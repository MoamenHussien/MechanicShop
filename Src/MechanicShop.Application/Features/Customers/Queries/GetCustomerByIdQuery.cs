using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MechanicShop.Application.Common.Constants;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : ICachedQuery<Result<CustomerDto>>
{
    public string CacheKey => $"Customer-{CustomerId}";

    public string[] Tags => [CacheTags.Customers];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(n => n.CustomerId).IdRequired("Customer");
    }
}

public class GetCustomerByIdQueryHandler(ILogger<GetCustomerByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.AsNoTracking()
                       .Where(c => c.Id == request.CustomerId)
                       .Select(CustomerMapper.CustomerProjection)
                       .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("Customer with Id {CustomerId} was not found", request.CustomerId);
            return ApplicationErrors.TheCustomerNotFound;
        }

        logger.LogInformation("Cache miss. Returning customer with Id {CustomerId}", request.CustomerId);
        return customer;
    }
}