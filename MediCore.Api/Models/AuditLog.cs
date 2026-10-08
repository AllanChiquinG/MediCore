namespace MediCore.Api.Models;

// Bitacora de accesos y cambios (riesgo R11).
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;     // ej. READ, CREATE, UPDATE
    public string EntityType { get; set; } = string.Empty; // ej. Patient
    public string? EntityId { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
