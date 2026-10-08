namespace MediCore.Api.Models;

// Evento pendiente de publicar en RabbitMQ. Se guarda en la misma transaccion que el dato
// clinico, asi no se pierde informacion si el broker esta caido (riesgo R13).
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;      // ej. LabResultReady
    public string Payload { get; set; } = "{}";            // JSON
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
