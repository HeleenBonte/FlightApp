using FlightApp.Domains.EntitiesDB;

namespace FlightApp.ViewModels
{
    public class BookingVM
    {
        public int BookingId { get; set; }
        public string? UserName { get; set; }
        public DateOnly BookingTime { get; set; }
        public bool PaymentStatus { get; set; }
        public int RouteId { get; set; }
        public int FlightId { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
        public string DepartureCity { get; set; } = "N/A";
        public string ArrivalCity { get; set; } = "N/A";
    }
}
