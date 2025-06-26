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
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
