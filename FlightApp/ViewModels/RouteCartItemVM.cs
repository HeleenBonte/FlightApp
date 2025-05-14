using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightApp.ViewModels
{
    public class RouteCartItemVM
    {
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public string Layover1 { get; set; }
        public string Layover2 { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public List<FlightVM> Flights { get; set; } = new List<FlightVM>();
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
        public int PassengerCount { get; set; } = 1;
        public double TotalPrice { get; set; }
        public string? Notes { get; set; } 
        public bool IsComplete { get; set; } = false;
        public DateTime AddedToCartTime { get; set; } = DateTime.Now;

        public double GetTotalPrice()
        {
            if (Passengers == null || !Passengers.Any())
            {
                return Flights.Sum(f => f.Price ?? 0) * PassengerCount;
            }

            double total = 0;
            foreach (var passenger in Passengers)
            {
                double basePrice = Flights.Sum(f => f.Price ?? 0);
                total += basePrice * passenger.BookingClassPriceFactor;
            }

            return total;
        }
    }
}