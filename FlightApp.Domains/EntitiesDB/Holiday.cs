using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Holiday
{
    public int HolidayId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Name { get; set; } = null!;

    public double PriceFactor { get; set; }

    public int? CityId { get; set; }

    public virtual City? City { get; set; }
}
