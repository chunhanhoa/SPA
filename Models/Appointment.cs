namespace LoanSpa.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}