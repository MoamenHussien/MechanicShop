using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : ICachedQuery<Result<CustomerDto>>
{
    public string CacheKey => $"Customer-{CustomerId}";

    public string[] Tags => ["Customers"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(n=>n.CustomerId).IdRequired("Customer");
    }
}

public class GetCustomerByIdQueryHandler(ILogger<GetCustomerByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.AsNoTracking()
            .Where(c => c.Id == request.CustomerId) 
            .Include(c => c.vehicles)                 
            .FirstOrDefaultAsync(cancellationToken);  

        if (customer is null)
        {
          logger.LogWarning("The Customer Is Not Found With ID : {id}",request.CustomerId);
           return  ApplicationErrors.TheCustomerNotFound;
        }
        logger.LogInformation("The Cache Is Miss And We Return Customer Info With Id : {id}",request.CustomerId);
        return customer.ToDto();
    }
}