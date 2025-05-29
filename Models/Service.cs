namespace LoanSpa.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Picture { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }
        public string Features { get; set; }
        public string Process { get; set; }
        public string Notes { get; set; }
    }
}