using FlightApp.Domains.EntitiesDB;

namespace FlightApp.ViewModels
{
    public class FlightVM
    {
        public int FlightId { get; set; }
        public string? ArrivalCity { get; set; }
        public string? DepartureCity { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public double? Price { get; set; }

    }
}
