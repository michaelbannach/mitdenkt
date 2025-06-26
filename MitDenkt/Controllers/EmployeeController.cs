using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MitDenkt.Data;
using MitDenkt.Models;

namespace MitDenkt.Controllers;


[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly MitDenktContext _context;
    
    public EmployeeController(MitDenktContext context) => _context = context;

    [HttpGet]
    public async Task<IEnumerable<Employee>> Get()
    {
        return await _context.Employees.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> Get(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        return employee == null ? NotFound() : Ok(employee);
    }
}