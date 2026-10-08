namespace MediCore.Api.Dtos;

public record SlotDto(DateTimeOffset Start, DateTimeOffset End);

public record AvailabilityDto(Guid DoctorId, DateOnly Date, List<SlotDto> Slots);
