using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class BookingHistory
{
    public int HistoryId { get; set; }

    public string UserId { get; set; } = null!;

    public int BookingId { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
