using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Enums;

namespace backend.Models;

[Table("event_participants")]
public class EventParticipant : BaseModel
{
    [Required] 
    [Column("event_id")] 
    public required long EventId { get; set; }

    [Required] 
    [Column("user_id")] 
    public required long UserId { get; set; }

    [Column("is_organizer")]
    public bool IsOrganizer { get; set; } = false;

    [NotMapped] 
    private EventStatus StatusValue { get; set; } = EventStatus.Invited;

    [Required]
    [Column("status")]
    [MaxLength(50)]
    public string Status
    {
        get => StatusValue.ToString();
        set => StatusValue = Enum.Parse<EventStatus>(value, true);
    }

}
    