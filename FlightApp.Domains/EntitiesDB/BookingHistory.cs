using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class BookingHistory
{
    public string HistoryId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string BookingId { get; set; } = null!;
}
