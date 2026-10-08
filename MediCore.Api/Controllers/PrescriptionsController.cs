using MediCore.Api.Data;
using MediCore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("api/prescriptions")]
public class PrescriptionsController : ControllerBase
{
    private readonly MediCoreDbContext _db;

    public PrescriptionsController(MediCoreDbContext db) => _db = db;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PrescriptionDto>> GetById(Guid id)
    {
        var prescription = await _db.Prescriptions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PrescriptionDto(
                p.Id, p.PatientId, p.DoctorId, p.Medication, p.Dosage, p.Instructions, p.IssuedAt))
            .FirstOrDefaultAsync();

        return prescription is null ? NotFound() : Ok(prescription);
    }
}
