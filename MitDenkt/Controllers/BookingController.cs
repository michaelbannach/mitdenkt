using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MitDenkt.Data;
using MitDenkt.Models;
using MitDenkt.Dtos;

namespace MitDenkt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly MitDenktContext _context;

    public BookingController(MitDenktContext context)
    {
        _context = context;
    }

    // Alle Buchungen (z. B. für Admin oder später eingeschränkt für Kunden)
    [HttpGet]
    public async Task<IEnumerable<Booking>> GetAll() =>
        await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Employee)
            .ToListAsync();

    // Buchungen an einem bestimmten Tag
    [HttpGet("date/{date}")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetByDate(string date)
    {
        if (!DateTime.TryParse(date, out var targetDate))
            return BadRequest("Ungültiges Datum.");

        var start = targetDate.Date;
        var end = start.AddDays(1);

        var bookings = await _context.Bookings
            .Where(b => b.StartTime >= start && b.StartTime < end)
            .Include(b => b.Employee)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                EmployeeId = b.EmployeeId,
                EmployeeName = b.Employee.FirstName + " " + b.Employee.LastName,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                CanDelete = true // Optional: Später durch echten Kundenvergleich ersetzen
            })
            .ToListAsync();

        return bookings;
    }

  
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var email = User.Identity?.Name;
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer == null) return Unauthorized();

        var booking = new Booking
        {
            EmployeeId = dto.EmployeeId,
            CustomerId = customer.Id,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            BookingServices = dto.ServiceIds.Select(id => new BookingService
            {
                ServiceId = id
            }).ToList()
        };

        var conflict = await _context.Bookings.AnyAsync(b =>
            b.EmployeeId == booking.EmployeeId &&
            dto.StartTime < b.EndTime &&
            dto.EndTime > b.StartTime);

        if (conflict)
            return BadRequest("Mitarbeiter ist zu diesem Zeitpunkt bereits gebucht.");

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // Buchung löschen
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // ⚠️ In echter App: nur eigene Buchung löschbar
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
            return NotFound();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
