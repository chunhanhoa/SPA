using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_Spa.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Chờ xác nhận";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Quan hệ với bảng AppointmentService và AppointmentChair
        public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
        public ICollection<AppointmentChair> AppointmentChairs { get; set; } = new List<AppointmentChair>();
    }
}