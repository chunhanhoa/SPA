using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int CustomerId { get; set; }

    public virtual ICollection<AppointmentChair> AppointmentChairs { get; set; } = new List<AppointmentChair>();

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual Customer Customer { get; set; } = null!;
}
