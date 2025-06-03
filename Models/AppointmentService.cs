using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    public class AppointmentService
    {
        [Key]
        public int AppointmentServiceId { get; set; }
        
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        
        [Required]
        public int Quantity { get; set; } = 1;
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}