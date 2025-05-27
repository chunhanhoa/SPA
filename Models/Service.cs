using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string? ServiceName { get; set; }

    public string? Picture { get; set; }

    public decimal? Price { get; set; }

    public int? Duration { get; set; }
    
    // Thêm các thuộc tính còn thiếu
    public string? Description { get; set; }
    
    public string? Features { get; set; }
    
    public string? Process { get; set; }
    
    public string? Notes { get; set; }

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual ICollection<InvoicesService> InvoicesServices { get; set; } = new List<InvoicesService>();
}
