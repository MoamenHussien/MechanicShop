using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
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

public class UpdateCustomerCommandHandler(ILogger<UpdateCustomerCommandHandler> logger, HybridCache cache, IAppDbContext context)
: IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var Customer = await context.Customers.Where(n => n.Id == request.id).Include(n => n.vehicles).FirstOrDefaultAsync(cancellationToken);
        if (Customer is null)
        {
            logger.LogWarning("The Customer Is Not Found With ID : {id} For Update", request.id);
            return ApplicationErrors.TheCustomerNotFound;
        }

        var Email = request.email.Trim().ToLower();

        var IFExists = await context.Customers.AnyAsync(n => n.Id != request.id && n.Email == Email);
        if (IFExists)
        {
            logger.LogWarning("Customer creation aborted. Email already exists : {email}", Email);
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

        List<Vehicle> vehicles = new List<Vehicle>();

        foreach (var vehicle in request.Vehicles)
        {
            var VehicleId = vehicle.id ?? Guid.NewGuid();
            var CreatedVehicle = Vehicle.Create(VehicleId, vehicle.year, vehicle.LicensePlate, vehicle.VehicleModelId);

            if (CreatedVehicle.IsError)
            {
                logger.LogWarning("Error During Create Customer Vehicles : {@Errors}", CreatedVehicle.Errors);
                return CreatedVehicle.Errors;
            }

            vehicles.Add(CreatedVehicle.Value);
        }

        var UpdateCustomer = Customer.Update(request.name, Email, request.PhoneNumber);

        if (UpdateCustomer.IsError)
        {
            logger.LogWarning("Error During Update Customer Info: {@Errors}", UpdateCustomer.Errors);
            return UpdateCustomer.Errors;
        }

        var UpSertVehicles = Customer.UpSertVehicles(vehicles);

        if (UpSertVehicles.IsError)
        {
            logger.LogWarning("Error During UpSert Customer Vehicles : {@Errors}", UpSertVehicles.Errors);

            return UpSertVehicles.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("Customers", cancellationToken);

        logger.LogInformation("Successfully Update The Customer With ID : {id} And Removed Cache Tag With Name Customer", request.id);

        return Result.Updated;
    }
}