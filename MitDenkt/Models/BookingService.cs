namespace MitDenkt.Models;

public class BookingService
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; }

    public int ServiceId { get; set; }
    public Service Service { get; set; }
}