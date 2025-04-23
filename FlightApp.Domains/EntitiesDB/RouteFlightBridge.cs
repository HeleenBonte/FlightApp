using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class RouteFlightBridge
{
    public string RouteId { get; set; } = null!;

    public string FlightId { get; set; } = null!;
}
