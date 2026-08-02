using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record UpdateCustomerCommand(Guid id, string name, string email, string PhoneNumber, List<UpdateVehicleCommand> Vehicles) : IRequest<Result<Updated>>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(n => n.id).IdRequired("Customer");
        RuleFor(n => n.name).NotEmpty().WithMessage("Customer Name is Required")
        .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Name")
        .Length(3, 255).WithMessage("The Name length From 3 to 255 Char");

        RuleFor(n => n.email).MustBeValidEmail();
        RuleFor(n => n.PhoneNumber).MustBeValidPhone();
        RuleFor(n => n.Vehicles).NotNull().WithMessage("Vehicles List Cannot Be Null").Must(n => n != null && n.Count > 0).WithMessage("Customer Must Have At Least One Vehicle");
        RuleForEach(n => n.Vehicles).SetValidator(new UpdateVehicleCommandValidator());
    }
}

public class UpdateCustomerCommandHandler(ILogger<UpdateCustomerCommandHandler> logger, ICacheInvalidator cacheInvalidator, IAppDbContext context)
: IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.Where(n => n.Id == request.id).Include(n => n.vehicles).FirstOrDefaultAsync(cancellationToken);
        if (customer is null)
        {
            logger.LogWarning("The Customer Is Not Found With ID : {id} For Update", request.id);
            return ApplicationErrors.TheCustomerNotFound;
        }

        var email = request.email.Trim().ToLower();

        var iFExists = await context.Customers.AnyAsync(n => n.Id != request.id && n.Email == email);
        if (iFExists)
        {
            logger.LogWarning("Customer creation aborted. Email already exists : {email}", email);
            return ApplicationErrors.CustomerWithThisEmailIsAlreadyExists;
        }

        var vehicleModelIds = request.Vehicles.Select(v => v.VehicleModelId).Distinct().ToList();
        var existingModelsCount = await context.VehicleModels
            .CountAsync(m => vehicleModelIds.Contains(m.Id), cancellationToken);

        if (existingModelsCount != vehicleModelIds.Count)
        {
            logger.LogWarning("Some vehicle models were not found.");
            return ApplicationErrors.NotFoundTheVehicleModel;
        }

        List<Vehicle> vehicles = new List<Vehicle>();

        foreach (var vehicle in request.Vehicles)
        {
            var vehicleId = vehicle.id ?? Guid.NewGuid();
            var createdVehicle = Vehicle.Create(vehicleId, vehicle.year, vehicle.LicensePlate, vehicle.VehicleModelId);

            if (createdVehicle.IsError)
            {
                logger.LogWarning("Error During Create Customer Vehicles : {@Errors}", createdVehicle.Errors);
                return createdVehicle.Errors;
            }

            vehicles.Add(createdVehicle.Value);
        }

        var updateCustomer = customer.Update(request.name, email, request.PhoneNumber);

        if (updateCustomer.IsError)
        {
            logger.LogWarning("Error During Update Customer Info: {@Errors}", updateCustomer.Errors);
            return updateCustomer.Errors;
        }

        var upSertVehicles = customer.UpSertVehicles(vehicles);

        if (upSertVehicles.IsError)
        {
            logger.LogWarning("Error During UpSert Customer Vehicles : {@Errors}", upSertVehicles.Errors);

            return upSertVehicles.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.Customers, cancellationToken);

        logger.LogInformation("Successfully Update The Customer With ID : {id} And Removed Cache Tag With Name Customer", request.id);

        return Result.Updated;
    }
}
