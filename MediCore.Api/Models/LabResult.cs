namespace MediCore.Api.Models;

public class LabResult
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid? OrderedByDoctorId { get; set; }
    public Doctor? OrderedByDoctor { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string ResultValue { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTimeOffset ResultDate { get; set; } = DateTimeOffset.UtcNow;
}
