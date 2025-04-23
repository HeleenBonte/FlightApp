using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Flight
{
    public string FlightId { get; set; } = null!;

    public string ArrivalCity { get; set; } = null!;

    public string DepartureCity { get; set; } = null!;

    public DateOnly DepartureTime { get; set; }

    public DateOnly ArrivalTime { get; set; }

    public int Seating { get; set; }

    public double Price { get; set; }

    public virtual City ArrivalCityNavigation { get; set; } = null!;

    public virtual City DepartureCityNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
}
