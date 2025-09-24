namespace backend.Dtos;

public class EventDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StartDateTime { get; set; } = "";
    public string EndDateTime { get; set; } = "";

    public string TimeZone { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
}