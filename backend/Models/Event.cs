using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("events")]
public class Event : BaseModel
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public required string Title { get; set; }
    
    [Column("description", TypeName = "text")]
    public string? Description { get; set; }
    
    [Required]
    [Column("start_time")]
    public required DateTime StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public required DateTime EndTime { get; set; }

    [Column("timezone")] 
    [MaxLength(255)]
    public string? TimeZone { get; set; } = "Asia/Kolkata";
}