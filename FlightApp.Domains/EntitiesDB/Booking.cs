using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Booking
{
    public string BookingId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateOnly BookingTime { get; set; }

    public bool PaymentStatus { get; set; }

    public string RouteId { get; set; } = null!;

    public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();

    public virtual Route Route { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
}
