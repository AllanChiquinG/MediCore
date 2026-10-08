using MediCore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Data;

// Datos de prueba INVENTADOS, solo para desarrollo. Nunca datos reales de pacientes.
public static class DbSeeder
{
    // IDs fijos de pacientes de prueba, para poder usarlos en archivos .http y pruebas.
    public static readonly Guid PatientOneId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid PatientTwoId = Guid.Parse("22222222-2222-2222-2222-222222222202");

    public static readonly Guid PrescriptionOneId = Guid.Parse("33333333-3333-3333-3333-333333333301");

    public static async Task SeedAsync(MediCoreDbContext db)
    {
        if (!await db.Doctors.AnyAsync()) SeedDoctors(db);
        if (!await db.Patients.AnyAsync()) SeedPatients(db);

        await db.SaveChangesAsync();

        if (!await db.Prescriptions.AnyAsync()) await SeedPrescriptionAsync(db);
    }

    // Una cita pasada + consulta + receta de prueba (datos inventados).
    private static async Task SeedPrescriptionAsync(MediCoreDbContext db)
    {
        var doctor = await db.Doctors.OrderBy(d => d.LicenseNumber).FirstAsync();
        var start = new DateTimeOffset(2026, 9, 1, 15, 0, 0, TimeSpan.Zero);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = PatientOneId,
            DoctorId = doctor.Id,
            StartTime = start,
            EndTime = start.AddMinutes(30),
            Status = AppointmentStatus.Completed,
            Reason = "Control general"
        };
        var consultation = new Consultation
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointment.Id,
            Diagnosis = "Resfriado comun"
        };

        db.Appointments.Add(appointment);
        db.Consultations.Add(consultation);
        db.Prescriptions.Add(new Prescription
        {
            Id = PrescriptionOneId,
            ConsultationId = consultation.Id,
            PatientId = PatientOneId,
            DoctorId = doctor.Id,
            Medication = "Paracetamol 500 mg",
            Dosage = "1 tableta cada 8 horas",
            Instructions = "Por 3 dias"
        });

        await db.SaveChangesAsync();
    }

    private static void SeedDoctors(MediCoreDbContext db)
    {
        var data = new[]
        {
            ("Ana",   "Morales", "Pediatria",        "MED-0001"),
            ("Luis",  "Perez",   "Cardiologia",      "MED-0002"),
            ("Marta", "Gomez",   "Medicina General", "MED-0003"),
        };

        foreach (var (first, last, specialty, license) in data)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = $"{first}.{last}@medicore.test".ToLowerInvariant(),
                PasswordHash = "SEED-SIN-LOGIN", // todavia no hay autenticacion
                Role = UserRole.Medico
            };

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FirstName = first,
                LastName = last,
                Specialty = specialty,
                LicenseNumber = license
            };

            // Lunes a viernes, 08:00 a 16:00, citas de 30 minutos
            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
            {
                doctor.Schedules.Add(new DoctorSchedule
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = day,
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(16, 0),
                    SlotMinutes = 30
                });
            }

            db.Users.Add(user);
            db.Doctors.Add(doctor);
        }
    }

    private static void SeedPatients(MediCoreDbContext db)
    {
        var data = new[]
        {
            (PatientOneId, "Carlos", "Lopez",   new DateOnly(1990, 5, 14)),
            (PatientTwoId, "Sofia",  "Ramirez", new DateOnly(1985, 11, 2)),
        };

        foreach (var (id, first, last, birth) in data)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = $"{first}.{last}@medicore.test".ToLowerInvariant(),
                PasswordHash = "SEED-SIN-LOGIN",
                Role = UserRole.Paciente
            };

            db.Users.Add(user);
            db.Patients.Add(new Patient
            {
                Id = id,
                UserId = user.Id,
                FirstName = first,
                LastName = last,
                DateOfBirth = birth
            });
        }
    }
}
