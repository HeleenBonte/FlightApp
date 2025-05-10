using FlightApp.Domains.EntitiesDB;

namespace FlightApp.ViewModels
{
    public class BookingHistoryVM
    {
        public int BookingId { get; set; }
        public bool PaymentStatus { get; set; }
        public DateOnly BookingTime { get; set; }
        public IEnumerable<PassengerVM>? Passengers { get; set; } = new List<PassengerVM>();
        public string DepartureCity { get; set; } 
        public string ArrivalCity { get; set; }
        public City ArrivalCityData { get; set; }
        public DateTime DepartureTime { get; set; }
    }
}
