using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int InvoiceDetailId { get; set; }
        
        [ForeignKey("Invoice")]
        public int InvoiceId { get; set; }
        
        public int? ServiceId { get; set; }
        
        public int? ChairId { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        
        public string? Notes { get; set; }
        
        public virtual Invoice Invoice { get; set; }
        
        [ForeignKey("ChairId")]
        public virtual Chair Chair { get; set; }
        
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
    } // Đóng class InvoiceDetail
} // Đóng namespace (nếu có)

