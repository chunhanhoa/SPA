using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class AppointmentChair
{
    public int AppointmentId { get; set; }

    public int ChairId { get; set; }

    public int? Quantity { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Chair Chair { get; set; } = null!;
}
