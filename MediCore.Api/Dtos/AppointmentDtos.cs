namespace MediCore.Api.Dtos;

public record CreateAppointmentRequest(Guid PatientId, Guid DoctorId, DateTimeOffset StartTime, string? Reason);

public record AppointmentDto(Guid Id, Guid PatientId, Guid DoctorId, DateTimeOffset Start, DateTimeOffset End, string Status);
