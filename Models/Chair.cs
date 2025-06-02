using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    public class Chair
    {
        [Key]
        public int ChairId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ChairName { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [Required]
        public int RoomId { get; set; }
        
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }
    }
}