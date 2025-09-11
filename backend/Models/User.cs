using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("users")]
public class User : BaseModel
{
     [Key]
     [Column("id")]
     public long Id { get; set; }
     
     [Required]
     [Column("name")]
     [MaxLength(255)]
     public required string Name { get; set; }
     
     [Required]
     [Column("email")]
     [MaxLength(320)]
     public required string Email { get; set; }
}