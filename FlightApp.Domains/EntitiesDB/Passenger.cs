using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Passenger
{
    public int PassengerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly Birthdate { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
