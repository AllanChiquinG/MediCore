using System.Text.Json;
using MediCore.Api.Data;
using MediCore.Api.Dtos;
using MediCore.Api.Models;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController(MediCoreDbContext db) : ControllerBase
{
    // POST /api/appointments
    // NOTA: por ahora el paciente viene en el cuerpo. Cuando agreguemos autenticacion (JWT),
    // el paciente se tomara del token para evitar que alguien reserve a nombre de otro (riesgo R01).
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var startUtc = request.StartTime.ToUniversalTime();

        if (startUtc <= DateTimeOffset.UtcNow)
            return BadRequest("La cita debe ser en una fecha y hora futuras.");

        if (request.Reason is { Length: > 500 })
            return BadRequest("El motivo no puede superar 500 caracteres.");

        if (!await db.Patients.AsNoTracking().AnyAsync(p => p.Id == request.PatientId, ct))
            return NotFound("Paciente no encontrado.");

        if (!await db.Doctors.AsNoTracking().AnyAsync(d => d.Id == request.DoctorId, ct))
            return NotFound("Medico no encontrado.");

        // El horario debe existir en la agenda del medico (dia y hora exactos de un bloque).
        var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(startUtc, ClinicTime.Zone).DateTime);
        var schedules = await db.DoctorSchedules.AsNoTracking()
            .Where(s => s.DoctorId == request.DoctorId && s.DayOfWeek == day.DayOfWeek)
            .ToListAsync(ct);

        var slot = SlotCalculator.BuildSlots(schedules, day)
            .FirstOrDefault(s => s.Start.ToUniversalTime() == startUtc);

        if (slot is null)
            return BadRequest("Ese horario no existe en la agenda del medico.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            StartTime = slot.Start.ToUniversalTime(),
            EndTime = slot.End.ToUniversalTime(),
            Status = AppointmentStatus.Scheduled,
            Reason = request.Reason
        };

        db.Appointments.Add(appointment);

        // Evento pendiente de publicar en RabbitMQ. Se guarda en la MISMA transaccion que la cita:
        // si la cita se guarda, el evento tambien; si falla, ninguno.
        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "AppointmentCreated",
            Payload = JsonSerializer.Serialize(new
            {
                appointmentId = appointment.Id,
                patientId = appointment.PatientId,
                doctorId = appointment.DoctorId,
                startTime = appointment.StartTime
            })
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // El indice unico (medico + horario) de la base de datos evita la doble reserva,
            // incluso si dos solicitudes llegan exactamente al mismo tiempo.
            return Conflict("Ese horario ya fue reservado por otro paciente.");
        }

        var dto = new AppointmentDto(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            TimeZoneInfo.ConvertTime(appointment.StartTime, ClinicTime.Zone),
            TimeZoneInfo.ConvertTime(appointment.EndTime, ClinicTime.Zone),
            appointment.Status.ToString());

        return StatusCode(StatusCodes.Status201Created, dto);
    }
}
