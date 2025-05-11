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
        public IEnumerable<Ticket>? Tickets { get; set; } = new List<Ticket>();
        public int seating { get; set; }
        public string? Notes { get; set; }
    }
}
