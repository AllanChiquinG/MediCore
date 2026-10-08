namespace MediCore.Api.Dtos;

public record PrescriptionDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    string Medication,
    string Dosage,
    string? Instructions,
    DateTimeOffset IssuedAt);
