using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
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
            .Must(n => n != null && n.Count > 0).WithMessage("Customer Must Have At Least One Vehicle");

        RuleForEach(n => n.Vehicles)
            .SetValidator(new CreateVehicleCommandValidator());
    }
}

public class CreateCustomerCommandHandler(
    ILogger<CreateCustomerCommandHandler> logger,
    ICacheInvalidator cacheInvalidator,
    IAppDbContext context) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var email = request.email.Trim().ToLowerInvariant();

        if (await context.Customers.AnyAsync(n => n.Email == email, cancellationToken))
        {
            logger.LogWarning("Customer with email {Email} already exists.", email);
            return ApplicationErrors.CustomerWithThisEmailIsAlreadyExists;
        }

        var vehicleModelIds = request.Vehicles
            .Select(v => v.VehicleModelId)
            .Distinct()
            .ToList();

        var existingModelIds = await context.VehicleModels
            .Where(vm => vehicleModelIds.Contains(vm.Id))
            .Select(vm => vm.Id)
            .ToListAsync(cancellationToken);

        if (existingModelIds.Count != vehicleModelIds.Count)
        {
            logger.LogWarning("Some vehicle models were not found.");
            return ApplicationErrors.NotFoundTheVehicleModel;
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
                logger.LogWarning("Failed to create vehicle domain model: {@Errors}", createdVehicle.Errors);
                return createdVehicle.Errors;
            }

            vehicles.Add(createdVehicle.Value);
        }

        var customer = Customer.Create(
            Guid.NewGuid(),
            request.name,
            request.email,
            request.PhoneNumber,
            vehicles);

        if (customer.IsError)
        {
            logger.LogWarning("Error while creating customer: {@Errors}", customer.Errors);

            return customer.Errors;
        }

        context.Customers.Add(customer.Value);

        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.Customers, cancellationToken);

        logger.LogInformation(
            "Successfully created customer with Id {CustomerId}. Removed Customers cache tag.",
            customer.Value.Id);


        var vehicleIds = customer.Value.vehicles
            .Select(v => v.Id)
            .ToList();

        await context.Vehicles
            .Where(v => vehicleIds.Contains(v.Id))
            .Include(v => v.VehicleModel)
                .ThenInclude(m => m.VehicleMake)
            .LoadAsync(cancellationToken);

        return customer.Value.ToDto();

    }
}
