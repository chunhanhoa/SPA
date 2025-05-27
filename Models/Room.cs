using System;
using System.Collections.Generic;

namespace LoanSpa.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string? RoomName { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual ICollection<Chair> Chairs { get; set; } = new List<Chair>();
}
