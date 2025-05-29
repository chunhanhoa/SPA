using LoanSpa.Models;

namespace QL_Spa.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}