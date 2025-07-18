namespace MitDenkt.Models;

public class Booking
{
    public int Id { get; set; }

    // Foreign Keys
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }

    // Navigation Properties -> Ermöglicht es Entity Framework die Relationen zu erkennen
    public Customer? Customer { get; set; }
    public Employee? Employee { get; set; }

    // Zeiträume der Buchungen
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    
    
    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();

}
