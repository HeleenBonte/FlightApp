using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class Passenger
{
    public string PassengerId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly Birthdate { get; set; }

    public string Country { get; set; } = null!;

    public string Email { get; set; } = null!;
}
