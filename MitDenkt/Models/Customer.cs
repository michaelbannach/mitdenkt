using Microsoft.AspNetCore.Identity;

namespace MitDenkt.Models;

public class Customer : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Buchungen des Kunden
    public ICollection<Booking>? Bookings { get; set; }
}