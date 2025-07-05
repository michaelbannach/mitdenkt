using Microsoft.AspNetCore.Mvc;
using MitDenkt.Services;
using MitDenkt.Dtos;

namespace MitDenkt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly BookingManager _bookingManager;

    public BookingController(BookingManager bookingManager)
    {
        _bookingManager = bookingManager;
    }

    // Alle Buchungen
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await _bookingManager.GetAllAsync();
        return Ok(bookings);
    }

    // Buchungen eines Tages abrufen
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetByDate(string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest("Ungültiges Datum.");

        var bookings = await _bookingManager.GetByDateAsync(parsedDate);
        return Ok(bookings);
    }

    // Buchung erstellen
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized();

        var result = await _bookingManager.CreateAsync(dto, email);
        return result.Success ? Ok() : BadRequest(result.ErrorMessage);
    }

    // Buchung löschen
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _bookingManager.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}