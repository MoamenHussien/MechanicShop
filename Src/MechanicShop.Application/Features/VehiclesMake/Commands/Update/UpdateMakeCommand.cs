using System.Data;
using System.Net.Http.Headers;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record UpdateMakeCommand(Guid id, string Make, List<UpdateModelCommand> Models) : IRequest<Result<Updated>>;

public class UpdateMakeCommandValidator : AbstractValidator<UpdateMakeCommand>
{
    public UpdateMakeCommandValidator()
    {
        RuleFor(n => n.id).IdRequired("Make");
        RuleFor(n => n.Make).NotEmpty().Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("You Must Enter Vehicle Make");
        RuleFor(n => n.Models).NotNull().Must(n => n != null && n.Count() > 0).WithMessage("You Must Enter At Least One Model");
        RuleForEach(n => n.Models).SetValidator(new UpdateModelCommandValidator());
    }
}

public class UpdateMakeCommandHandler(IAppDbContext context, ILogger<CreateMakeCommandHandler> logger, ICacheInvalidator cacheInvalidator)
          : IRequestHandler<UpdateMakeCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateMakeCommand request, CancellationToken cancellationToken)
    {
        var make = await context.VehicleMakes.Include(n => n.VehicleModels)
                                      .FirstOrDefaultAsync(n => n.Id == request.id, cancellationToken);

        if (make is not null)
        {
            var isMakeExists = await context.VehicleMakes.AnyAsync(n => n.Id != request.id && n.Make.ToLower() == request.Make.ToLower(), cancellationToken);

            if (isMakeExists)
            {
                logger.LogWarning("Make name '{Make}' already exists for another VehicleMake.", request.Make);
                return VehicleMakeErrors.MakeIsAlreadyExists;
            }

            var updateResult = make.Update(request.Make);

            if (updateResult.IsError)
            {
                logger.LogWarning("Update Make failed for Id = {id}: {Error}", request.id, updateResult.TopError.Description);
                return updateResult.Errors;
            }

            var upVehicleModel = new List<VehicleModel>();

            foreach (var item in request.Models)
            {
                var upModel = VehicleModel.Create(item.ModelId ?? Guid.NewGuid(), item.model);
                if (upModel.IsError)
                {
                    logger.LogWarning("VehicleModel creation failed during UpdateMake for Model '{Model}': {Error}", item.model, upModel.TopError.Description);

                    return upModel.Errors;
                }

                upVehicleModel.Add(upModel.Value);
            }

            var updateModelsResult = make.UpSertModels(upVehicleModel);
            if (updateModelsResult.IsError)
            {
                logger.LogWarning("Upsert Models failed for Make Id = {id}", request.id);

                return updateModelsResult.Errors;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            logger.LogError("The vehicle Make Is Not Found With Id = {id}", request.id);

            return ApplicationErrors.MakeNotFound;
        }

        await cacheInvalidator.EvictByTagAsync(CacheTags.VehicleMakes, cancellationToken);

        logger.LogInformation("The Hybrid Cache Delete The Tag With Name VMakes");

        return Result.Updated;
    }
}
