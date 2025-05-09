using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class City
{
    public int CityId { get; set; }

    public string CityName { get; set; } = null!;

    public int? ApiId { get; set; }

    public virtual ICollection<Flight> FlightArrivalCityNavigations { get; set; } = new List<Flight>();

    public virtual ICollection<Flight> FlightDepartureCityNavigations { get; set; } = new List<Flight>();

    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();

    public virtual ICollection<MealChoice> MealChoices { get; set; } = new List<MealChoice>();

    public virtual ICollection<Route> RouteArrivalCities { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteDepartureCities { get; set; } = new List<Route>();
}
