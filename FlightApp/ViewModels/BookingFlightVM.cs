using FlightApp.Domains.EntitiesDB;

namespace FlightApp.ViewModels
{
    public class BookingFlightVM
    {
        public City ArrivalCity { get; set; }
        public City DepartureCity { get; set; }
        public DateOnly DepartureDay { get; set; }
    }
}
