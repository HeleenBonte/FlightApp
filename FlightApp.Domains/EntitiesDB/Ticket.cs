    using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int FlightId { get; set; }

    public int BookingClassId { get; set; }

    public int PassengerId { get; set; }

    public int SeatNumber { get; set; }

    public int MealChoiceId { get; set; }

    public virtual BookingClass BookingClass { get; set; } = null!;

    public virtual Flight Flight { get; set; } = null!;

    public virtual MealChoice MealChoice { get; set; } = null!;

    public virtual Passenger Passenger { get; set; } = null!;
}
