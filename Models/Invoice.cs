using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QL_Spa.Models; // Thêm namespace chứa InvoiceDetail (nếu cần)

namespace QL_Spa.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Discount { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalAmount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Chờ thanh toán";
        
        // Navigation property
        public ICollection<InvoiceService> InvoiceServices { get; set; }
        
        // Sửa thuộc tính InvoiceDetails để sử dụng đúng tên model
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
        
        public Invoice()
        {
            InvoiceServices = new List<InvoiceService>();
        }
    }
}