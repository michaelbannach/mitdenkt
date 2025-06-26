using Microsoft.AspNetCore.Identity;

namespace MitDenkt.Models;

public class Customer : IdentityUser<int>
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

  //Buchungen des Kunden
    public ICollection<Booking>? Bookings { get; set; }
}
