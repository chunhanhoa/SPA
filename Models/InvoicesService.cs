using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class InvoicesService
{
    public int InvoiceId { get; set; }

    public int ServiceId { get; set; }

    public int Quantity { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
