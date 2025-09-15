namespace backend.Requests;

public class EventRequest
{
    public long? UserId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? EventDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? TimeZone { get; set; }
}