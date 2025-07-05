namespace MitDenkt.Dtos;

public class CreateBookingDto
{
    public int EmployeeId { get; set; }
    public DateTime StartTime { get; set; }
    
    public List<int> ServiceIds { get; set; } = new();
}