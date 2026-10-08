namespace MediCore.Api.Models;

public enum UserRole
{
    Admin,
    Medico,
    Paciente,
    Laboratorio
}

public enum AppointmentStatus
{
    Scheduled,
    Cancelled,
    Completed
}
