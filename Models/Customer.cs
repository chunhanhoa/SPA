using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string UserId { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public DateTime? CreatedDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual AspNetUser User { get; set; } = null!;
}
