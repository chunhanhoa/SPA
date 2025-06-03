using System.Collections.Generic;

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
        
        // Thêm navigation property
        public ICollection<InvoiceService> InvoiceServices { get; set; }
        
        public Invoice()
        {
            InvoiceServices = new List<InvoiceService>();
        }
    }
}