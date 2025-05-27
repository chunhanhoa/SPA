using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class Chair
{
    public int ChairId { get; set; }

    public string? ChairName { get; set; }

    public bool? IsAvailable { get; set; }

    public int? RoomId { get; set; }

    public virtual ICollection<AppointmentChair> AppointmentChairs { get; set; } = new List<AppointmentChair>();

    public virtual Room? Room { get; set; }
}
