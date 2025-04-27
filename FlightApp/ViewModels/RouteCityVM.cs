using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlightApp.ViewModels
{
    public class RouteCityVM
    {
        public int ArrivalCityID { get; set; }
        public IEnumerable<SelectListItem>? Cities { get; set; }
        public int DepartureCityID { get; set; }
        public IEnumerable<RouteVM>? Routes { get; set; }
    }
}
