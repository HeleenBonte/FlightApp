using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Holiday
{
    public string HolidayId { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Name { get; set; } = null!;

    public double PriceFactor { get; set; }
}
