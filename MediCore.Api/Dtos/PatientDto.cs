namespace MediCore.Api.Dtos;

public record PatientDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Phone);
