using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class BookingPassenger
{
    public string BookingId { get; set; } = null!;

    public string PassengerId { get; set; } = null!;
}
