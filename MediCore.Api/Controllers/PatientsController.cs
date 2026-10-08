using MediCore.Api.Data;
using MediCore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly MediCoreDbContext _db;

    public PatientsController(MediCoreDbContext db) => _db = db;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientDto>> GetById(Guid id)
    {
        var patient = await _db.Patients
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatientDto(p.Id, p.FirstName, p.LastName, p.DateOfBirth, p.Phone))
            .FirstOrDefaultAsync();

        return patient is null ? NotFound() : Ok(patient);
    }
}
