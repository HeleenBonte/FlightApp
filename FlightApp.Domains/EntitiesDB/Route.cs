using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Route
{
    public int RouteId { get; set; }

    public DateTime DepartureTime { get; set; }

    public int DepartureCityId { get; set; }

    public DateTime ArrivalTime { get; set; }

    public int ArrivalCityId { get; set; }

    public virtual City ArrivalCity { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual City DepartureCity { get; set; } = null!;

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
