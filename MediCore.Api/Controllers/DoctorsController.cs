using MediCore.Api.Data;
using MediCore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController(MediCoreDbContext db) : ControllerBase
{
    // GET /api/doctors  o  GET /api/doctors?specialty=Pediatria
    [HttpGet]
    public async Task<ActionResult<List<DoctorDto>>> GetAll([FromQuery] string? specialty, CancellationToken ct)
    {
        var query = db.Doctors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            var term = specialty.Trim().ToLower();
            query = query.Where(d => d.Specialty.ToLower() == term);
        }

        var doctors = await query
            .OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
            .Select(d => new DoctorDto(d.Id, d.FirstName, d.LastName, d.Specialty))
            .ToListAsync(ct);

        return Ok(doctors);
    }
}
