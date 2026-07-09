using MediatR;

public sealed record CreateVehicleModelCommand (string model) : IRequest<Result<VehicleModelDto>>;
