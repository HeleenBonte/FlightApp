using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Booking
{
    public int BookingId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime BookingTime { get; set; }

    public bool PaymentStatus { get; set; }

    public int? RouteId { get; set; }

    public int? FlightId { get; set; }

    public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();

    public virtual Flight? Flight { get; set; }

    public virtual Route? Route { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
}
