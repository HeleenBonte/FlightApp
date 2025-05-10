using FlightApp.Domains.EntitiesDB;
using FlightApp.ViewModels;

public class BookingHistoryVM
{
    public int BookingId { get; set; }
    public bool PaymentStatus { get; set; }
    public DateTime BookingTime { get; set; }
    public IEnumerable<PassengerVM>? Passengers { get; set; } = new List<PassengerVM>();
    public string DepartureCity { get; set; }
    public string ArrivalCity { get; set; }
    public City? ArrivalCityData { get; set; }
    public int? RouteId { get; set; }
    public int? FlightId { get; set; }
    public bool IsDirectFlight => FlightId.HasValue && !RouteId.HasValue;
    public string BookingType => IsDirectFlight ? "Direct Flight" : "Route with Connections";
}