using MediCore.Api.Data;
using MediCore.Api.Dtos;
using MediCore.Api.Models;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController(MediCoreDbContext db) : ControllerBase
{
    // GET /api/doctors  o  GET /api/doctors?specialty=Pediatria
    [HttpGet]
    public async Task<ActionResult<List<DoctorDto>>> GetAll([FromQuery] string? specialty, CancellationToken ct)
    {
        var query = db.Doctors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            var term = specialty.Trim().ToLower();
            query = query.Where(d => d.Specialty.ToLower() == term);
        }

        var doctors = await query
            .OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
            .Select(d => new DoctorDto(d.Id, d.FirstName, d.LastName, d.Specialty))
            .ToListAsync(ct);

        return Ok(doctors);
    }

    // GET /api/doctors/{id}/availability?date=2026-10-12
    // Devuelve los horarios libres del medico en esa fecha (hora de la clinica).
    [HttpGet("{id:guid}/availability")]
    public async Task<ActionResult<AvailabilityDto>> GetAvailability(Guid id, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        if (date is null)
            return BadRequest("Falta el parametro 'date' (formato yyyy-MM-dd).");

        var day = date.Value;

        if (!await db.Doctors.AsNoTracking().AnyAsync(d => d.Id == id, ct))
            return NotFound();

        var schedules = await db.DoctorSchedules.AsNoTracking()
            .Where(s => s.DoctorId == id && s.DayOfWeek == day.DayOfWeek)
            .OrderBy(s => s.StartTime)
            .ToListAsync(ct);

        // Todos los horarios posibles del dia, segun la agenda del medico.
        var allSlots = SlotCalculator.BuildSlots(schedules, day);

        // Citas ya ocupadas ese dia (las canceladas liberan el horario).
        var midnightLocal = day.ToDateTime(TimeOnly.MinValue);
        var dayStartUtc = new DateTimeOffset(midnightLocal, ClinicTime.Zone.GetUtcOffset(midnightLocal)).ToUniversalTime();
        var dayEndUtc = dayStartUtc.AddDays(1);

        var booked = (await db.Appointments.AsNoTracking()
                .Where(a => a.DoctorId == id
                            && a.Status != AppointmentStatus.Cancelled
                            && a.StartTime >= dayStartUtc
                            && a.StartTime < dayEndUtc)
                .Select(a => a.StartTime)
                .ToListAsync(ct))
            .ToHashSet();

        var now = DateTimeOffset.UtcNow;
        var free = allSlots
            .Where(s => s.Start > now && !booked.Contains(s.Start))
            .ToList();

        return Ok(new AvailabilityDto(id, day, free));
    }
}
