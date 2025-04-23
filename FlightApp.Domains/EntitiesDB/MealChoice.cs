using System;
using System.Collections.Generic;

namespace FlightApp.Domains.EntitiesDB;

public partial class MealChoice
{
    public int MealChoiceId { get; set; }

    public string Type { get; set; } = null!;

    public int? CityId { get; set; }

    public virtual City? City { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
