namespace LoanSpa.Models
{
    public class Service
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public List<string> Benefits { get; set; }
        public List<string> Process { get; set; }
        public string Note { get; set; }
    }
}
