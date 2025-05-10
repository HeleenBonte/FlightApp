using System.Security.Permissions;

namespace FlightApp.ViewModels
{
    public class RouteListHotelListVM
    {
        public List<RouteVM> routes { get; set; }
        public List<HotelVM> hotels { get; set; }
    }
}
