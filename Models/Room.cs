using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    [Table("Rooms")] // Explicitly specify the table name
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        
        [Required]
        public string RoomName { get; set; }
        
        [Required]
        public bool IsAvailable { get; set; }
    }
}