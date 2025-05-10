using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Flight
{
    public int FlightId { get; set; }

    public int ArrivalCity { get; set; }

    public int DepartureCity { get; set; }

    public DateTime? DepartureTime { get; set; }

    public DateTime? ArrivalTime { get; set; }

    public int Seating { get; set; }

    public double Price { get; set; }

    public virtual City ArrivalCityNavigation { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual City DepartureCityNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
}
