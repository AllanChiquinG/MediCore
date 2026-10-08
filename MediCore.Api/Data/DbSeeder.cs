using MediCore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Data;

// Datos de prueba INVENTADOS, solo para desarrollo. Nunca datos reales de pacientes.
public static class DbSeeder
{
    public static async Task SeedAsync(MediCoreDbContext db)
    {
        if (await db.Doctors.AnyAsync()) return;

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

        await db.SaveChangesAsync();
    }
}
