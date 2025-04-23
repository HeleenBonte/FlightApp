using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Route
{
    public string RouteId { get; set; } = null!;

    public DateOnly DepartureTime { get; set; }

    public string DepartureCityId { get; set; } = null!;

    public DateOnly ArrivalTime { get; set; }

    public string ArrivalCityId { get; set; } = null!;

    public virtual City ArrivalCity { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual City DepartureCity { get; set; } = null!;

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
