using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Booking
{
    public string BookingId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateOnly BookingTime { get; set; }

    public bool PaymentStatus { get; set; }

    public string RouteCode { get; set; } = null!;
}
