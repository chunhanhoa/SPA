namespace QL_Spa.Models
{
    public class AppointmentChair
    {
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        public int ChairId { get; set; }
        public Chair Chair { get; set; }
        public int Quantity { get; set; }
    }
}