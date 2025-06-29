using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MitDenkt.Data;
using MitDenkt.Models;

namespace MitDenkt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceController : ControllerBase
{
    private readonly MitDenktContext _context;

    public ServiceController(MitDenktContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Service>>> GetAll()
    {
        var services = await _context.Services
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(services);
    }
}