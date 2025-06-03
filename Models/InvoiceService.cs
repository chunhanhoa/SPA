using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    public class InvoiceService
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}