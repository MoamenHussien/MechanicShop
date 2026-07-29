using FluentValidation;
using MediatR;

public sealed record UpdateVehicleCommand(Guid? id, int year, string LicensePlate, Guid VehicleModelId) : IRequest<Result<Updated>>;

public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(n => n.year).NotEmpty().WithMessage("Year Is  required").Must(n => n > 1990).WithMessage("You Must Enter Year > 1990");
        RuleFor(n => n.LicensePlate).NotEmpty().WithMessage("License Plate Is  required")
          .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid License Plate")
          .Length(3, 8).WithMessage("The LicensePlate Length From 3 To 8 Char");
        RuleFor(n => n.VehicleModelId).IdRequired("VehicleMode");
    }

}
