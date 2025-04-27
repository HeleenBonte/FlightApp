using FlightApp.Domains.EntitiesDB;

namespace FlightApp.ViewModels
{
    public class RouteVM
    {
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public string? DepartureCity { get; set; }
        public string? ArrivalCity { get; set; }
        public IEnumerable<Flight> Flights { get; set; } = new List<Flight>();

        public City? Layover1 { get; set; }
        public City? Layover2 { get; set; }
    }
}
