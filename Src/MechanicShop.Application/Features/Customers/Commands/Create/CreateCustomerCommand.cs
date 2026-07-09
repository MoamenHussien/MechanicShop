using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record CreateCustomerCommand(string name,string email,string PhoneNumber,List<CreateVehicleCommand> Vehicles):IRequest<Result<CustomerDto>>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(n=>n.name).NotEmpty().WithMessage("Customer Name is Required")
        .Must(n=> !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Name")
        .Length(3,255).WithMessage("The Name length From 3 to 255 Char");

        RuleFor(n=>n.email).MustBeValidEmail();
        RuleFor(n=>n.PhoneNumber).MustBeValidEmail();
        RuleFor(n=>n.Vehicles).NotNull().WithMessage("Vehicle list Cannot Be Null").Must(n=>n.Count>0).WithMessage("Customer Must Have At Least One Vehicle");
        RuleForEach(n=>n.Vehicles).SetValidator(new CreateVehicleCommandValidator());
    }
}

public class CreateCustomerCommandHandler(ILogger<CreateCustomerCommandHandler> logger, HybridCache cache, IAppDbContext context)
: IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var email = request.email.Trim().ToLower();
        var exists = await context.Customers.AnyAsync(n=>n.Email== email);
        if (exists)
        {
             logger.LogWarning("Customer creation aborted. Email already exists : {email}",email);
             return ApplicationErrors.CustomerExists;
        }

        List<Vehicle> vehicles = new List<Vehicle>();

        foreach(var vehicle in request.Vehicles)
        {
            var CreatedVehicles =Vehicle.Created(Guid.NewGuid(),vehicle.year,vehicle.LicensePlate,vehicle.VehicleModelId);
            if (CreatedVehicles.IsError)
            {
                logger.LogWarning("Error During creating Vehicle: {@Errors}", CreatedVehicles.Errors);
                return CreatedVehicles.Errors;
            }

            vehicles.Add(CreatedVehicles.Value);
        }
        var customer = Customer.Create(Guid.NewGuid(),request.name,email,request.PhoneNumber,vehicles);
        if (customer.IsError)
        {
        logger.LogWarning("Error During creating Customer: {@Errors}", customer.Errors);

            return customer.Errors;
        }


        await context.Customers.AddAsync(customer.Value);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("Customers",cancellationToken);

        logger.LogInformation("Successfully created New Customer And Removed Cache Tag With Name Customer , New Customer Id : {@CustomerId}", customer.Value.Id);

        return customer.Value.ToDto();
    }
}