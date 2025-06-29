namespace MitDenkt.Models;

public class Service
{
  
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;

        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }

