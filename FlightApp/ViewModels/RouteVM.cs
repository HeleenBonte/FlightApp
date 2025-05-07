using FlightApp.Domains.EntitiesDB;

namespace FlightApp.ViewModels
{
    public class RouteVM
    {
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string? DepartureCity { get; set; }
        public string? ArrivalCity { get; set; }
        public IEnumerable<FlightVM> Flights { get; set; } = new List<FlightVM>();

        public String? Layover1 { get; set; }
        public String? Layover2 { get; set; }
        public DateTime? ArrivalTime { get; set; } // Added property for ArrivalTime
    }
}
