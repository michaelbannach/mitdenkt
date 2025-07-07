namespace MitDenkt.Dtos;


    public class BookingDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public bool CanDelete { get; set; } = true; // für Auth später sinnvoll
    }

