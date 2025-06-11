using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    public class InvoiceService
    {
        [Required]
        public int InvoiceId { get; set; }
        
        [Required]
        public int ServiceId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        
        // Navigation properties
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
        
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }
}