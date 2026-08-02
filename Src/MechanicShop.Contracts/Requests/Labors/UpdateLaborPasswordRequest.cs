namespace MechanicShop.Contracts.Requests.Labors;

public sealed record UpdateLaborPasswordRequest(string CurrentPassword, string NewPassword);

public sealed record UpdateUserPasswordRequest(string CurrentPassword, string NewPassword);
