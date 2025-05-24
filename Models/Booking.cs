namespace LoanSpa.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ServiceId { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
