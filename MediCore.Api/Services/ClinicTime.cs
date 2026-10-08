using MediCore.Api.Dtos;
using MediCore.Api.Models;

namespace MediCore.Api.Services;

// Zona horaria de la clinica: los horarios de los medicos se definen en hora local.
public static class ClinicTime
{
    public static readonly TimeZoneInfo Zone = Load();

    private static TimeZoneInfo Load()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time"); }
    }
}

public static class SlotCalculator
{
    // Todos los horarios posibles de un dia segun la agenda semanal del medico (sin descontar citas).
    public static List<SlotDto> BuildSlots(IEnumerable<DoctorSchedule> schedules, DateOnly day)
    {
        var slots = new List<SlotDto>();

        foreach (var s in schedules.OrderBy(x => x.StartTime))
        {
            if (s.SlotMinutes <= 0) continue;

            var step = TimeSpan.FromMinutes(s.SlotMinutes);
            var endOfShift = s.EndTime.ToTimeSpan();

            for (var t = s.StartTime.ToTimeSpan(); t + step <= endOfShift; t += step)
            {
                var startLocal = day.ToDateTime(TimeOnly.MinValue).Add(t);
                var endLocal = startLocal.Add(step);
                slots.Add(new SlotDto(
                    new DateTimeOffset(startLocal, ClinicTime.Zone.GetUtcOffset(startLocal)),
                    new DateTimeOffset(endLocal, ClinicTime.Zone.GetUtcOffset(endLocal))));
            }
        }

        return slots;
    }
}
