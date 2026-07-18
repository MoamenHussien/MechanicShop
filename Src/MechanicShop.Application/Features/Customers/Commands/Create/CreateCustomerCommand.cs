using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record CreateCustomerCommand(
    string name,
    string email,
    string PhoneNumber,
    List<CreateVehicleCommand> Vehicles)
    : IRequest<Result<CustomerDto>>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(n => n.name)
            .NotEmpty().WithMessage("Customer Name is Required")
            .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Name")
            .Length(3, 255).WithMessage("The Name length From 3 to 255 Char");

        RuleFor(n => n.email)
            .MustBeValidEmail();

        RuleFor(n => n.PhoneNumber)
            .MustBeValidPhone();

        RuleFor(n => n.Vehicles)
            .NotNull().WithMessage("Vehicle list Cannot Be Null")
            .Must(n => n.Count > 0).WithMessage("Customer Must Have At Least One Vehicle");

        RuleForEach(n => n.Vehicles)
            .SetValidator(new CreateVehicleCommandValidator());
    }
}

public class CreateCustomerCommandHandler(
    ILogger<CreateCustomerCommandHandler> logger,
    IMediator mediator,
    HybridCache cache,
    IAppDbContext context) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request,CancellationToken cancellationToken)
    {
        var email = request.email.Trim().ToLowerInvariant();

        var exists = await context.Customers
            .AnyAsync(c => c.Email == email, cancellationToken);

        if (exists)
        {
            logger.LogWarning(
                "Customer creation aborted. Email already exists: {Email}",
                email);

            return ApplicationErrors.CustomerExists;
        }

        var vehicles = new List<Vehicle>(request.Vehicles.Count);

        foreach (var vehicle in request.Vehicles)
        {
            var createdVehicle = Vehicle.Create(
                Guid.NewGuid(),
                vehicle.year,
                vehicle.LicensePlate,
                vehicle.VehicleModelId);

            if (createdVehicle.IsError)
            {
                logger.LogWarning(
                    "Error while creating vehicle: {@Errors}",
                    createdVehicle.Errors);

                return createdVehicle.Errors;
            }

            vehicles.Add(createdVehicle.Value);
        }

        var customer = Customer.Create(Guid.NewGuid(),request.name,email,request.PhoneNumber,vehicles);

        if (customer.IsError)
        {
            logger.LogWarning("Error while creating customer: {@Errors}",customer.Errors);

            return customer.Errors;
        }

        context.Customers.Add(customer.Value);

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("Customers", cancellationToken);

        logger.LogInformation(
            "Successfully created customer with Id {CustomerId}. Removed Customers cache tag.",
            customer.Value.Id);

        return await mediator.Send(
            new GetCustomerByIdQuery(customer.Value.Id),
            cancellationToken);
    }
}