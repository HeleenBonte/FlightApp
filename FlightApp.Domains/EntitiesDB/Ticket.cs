using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Ticket
{
    public string TicketId { get; set; } = null!;

    public string FlightId { get; set; } = null!;

    public string BookingClassId { get; set; } = null!;

    public string PassengerId { get; set; } = null!;

    public int SeatNumber { get; set; }

    public int MealChoiceId { get; set; }
}
