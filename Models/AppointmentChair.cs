using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    public class AppointmentChair
    {
        public int AppointmentChairId { get; set; }
        
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        
        [ForeignKey("Chair")]
        public int ChairId { get; set; }
        public Chair Chair { get; set; }
    }
}