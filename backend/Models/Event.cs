using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("events")]
public class Event : BaseModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public long Id { get; set; }

    [Required] [Column("user_id")] 
    public long UserId { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public required string Title { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Required]
    [Column("event_date", TypeName = "date")]
    public DateOnly EventDate { get; set; }

    [Required]
    [Column("start_time", TypeName = "time")]
    public TimeOnly StartTime { get; set; }

    [Required]
    [Column("end_time", TypeName = "time")]
    public TimeOnly EndTime { get; set; }

    [Column("timezone")] [MaxLength(255)] 
    public string? TimeZone { get; set; } = "Asia/Kolkata";

    public void GenerateId()
    {
        Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}