using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FlightApp.ViewModels
{
    public class RouteCityVM
    {
        public int ArrivalCityID { get; set; }
        public IEnumerable<SelectListItem>? Cities { get; set; }
        public int DepartureCityID { get; set; }
        public IEnumerable<RouteVM>? Routes { get; set; }
        [DataType(DataType.Date)]
        public DateOnly DepartureDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(3));
    }
}
