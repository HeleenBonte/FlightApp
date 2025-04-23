using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class BookingClass
{
    public int BookingClassId { get; set; }

    public string Description { get; set; } = null!;

    public double PriceFactor { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
