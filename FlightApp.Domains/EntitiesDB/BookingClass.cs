using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class BookingClass
{
    public string BookingClassId { get; set; } = null!;

    public string Description { get; set; } = null!;

    public double PriceFactor { get; set; }
}
