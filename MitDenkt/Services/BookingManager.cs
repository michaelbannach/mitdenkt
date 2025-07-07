using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MitDenkt.Data;
using MitDenkt.Dtos;
using MitDenkt.Models;

namespace MitDenkt.Services;

public class BookingManager
{
    private readonly MitDenktContext _context;

    public BookingManager(MitDenktContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Employee)
            .ToListAsync();
    }

    public async Task<List<BookingDto>> GetByDateAsync(DateTimeOffset targetDate)
    {
        var berlinTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var offset = berlinTz.GetUtcOffset(targetDate.Date);

        var start = new DateTimeOffset(targetDate.Date, offset);
        var end = start.AddDays(1);

        var bookings = await _context.Bookings
            .Where(b => b.StartTime >= start && b.StartTime < end)
            .Include(b => b.Employee)
            .ToListAsync();

        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            EmployeeId = b.EmployeeId,
            EmployeeName = b.Employee.FirstName + " " + b.Employee.LastName,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            CanDelete = true
        }).ToList();
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateAsync(CreateBookingDto dto, string? userEmail)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
        if (customer == null) return (false, "Nicht angemeldet.");

        var services = await _context.Services
            .Where(s => dto.ServiceIds.Contains(s.Id))
            .ToListAsync();

        var totalMinutes = services.Sum(s => s.DurationMinutes);
        if (totalMinutes <= 0) return (false, "Ungültige Gesamtdauer.");

        var endTime = dto.StartTime.AddMinutes(totalMinutes);

        var conflict = await _context.Bookings.AnyAsync(b =>
            b.EmployeeId == dto.EmployeeId &&
            dto.StartTime < b.EndTime &&
            endTime > b.StartTime);

        if (conflict) return (false, "Mitarbeiter ist bereits gebucht.");

        var booking = new Booking
        {
            EmployeeId = dto.EmployeeId,
            CustomerId = customer.Id,
            StartTime = dto.StartTime,
            EndTime = endTime,
            BookingServices = services.Select(s => new BookingService
            {
                ServiceId = s.Id
            }).ToList()
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return false;

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        return true;
    }
}
