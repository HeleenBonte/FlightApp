using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class RouteFlightBridge
{
    public int RouteId { get; set; }

    public int FlightId { get; set; }
    public virtual Route RouteNav { get; set; } = null!;
    public virtual Flight FlightNav { get; set; } = null!;
}