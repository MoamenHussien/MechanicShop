namespace MechanicShop.Application.Features.Labors.DTOs;

public sealed record EmployeeDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    List<string> Roles,
    bool IsActive
);
