using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? Discount { get; set; }

    public decimal? FinalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public int? CustomerId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<InvoicesService> InvoicesServices { get; set; } = new List<InvoicesService>();
}
