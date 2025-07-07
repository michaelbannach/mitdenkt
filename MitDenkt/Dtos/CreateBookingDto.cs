namespace MitDenkt.Dtos;

public class CreateBookingDto
{
    public int EmployeeId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    
    public List<int> ServiceIds { get; set; } = new();
}