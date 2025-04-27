using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlightApp.ViewModels
{
    public class FlightsCityVM
    {
        public int ArrivalCityID { get; set; }
        public IEnumerable<SelectListItem>? Cities { get; set; }
        public int DepartureCityID { get; set; }
        public IEnumerable<FlightVM>? Flights { get; set; }
    }
}
