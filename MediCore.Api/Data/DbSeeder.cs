using MediCore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Data;

// Datos de prueba INVENTADOS, solo para desarrollo. Nunca datos reales de pacientes.
public static class DbSeeder
{
    // IDs fijos de pacientes de prueba, para poder usarlos en archivos .http y pruebas.
    public static readonly Guid PatientOneId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid PatientTwoId = Guid.Parse("22222222-2222-2222-2222-222222222202");

    public static async Task SeedAsync(MediCoreDbContext db)
    {
        if (!await db.Doctors.AnyAsync()) SeedDoctors(db);
        if (!await db.Patients.AnyAsync()) SeedPatients(db);

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
