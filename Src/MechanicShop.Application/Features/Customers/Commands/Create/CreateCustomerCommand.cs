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
            .Must(n => n != null && n.Count > 0).WithMessage("Customer Must Have At Least One Vehicle");

        RuleForEach(n => n.Vehicles)
            .SetValidator(new CreateVehicleCommandValidator());
    }
}

public class CreateCustomerCommandHandler(
    ILogger<CreateCustomerCommandHandler> logger,
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

        var vehicleModelIds = request.Vehicles.Select(v => v.VehicleModelId).Distinct().ToList();
        var existingModelsCount = await context.VehicleModels
            .CountAsync(m => vehicleModelIds.Contains(m.Id), cancellationToken);

        if (existingModelsCount != vehicleModelIds.Count)
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