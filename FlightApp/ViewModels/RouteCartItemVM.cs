using System;
using System.Collections.Generic;

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
        public DateTime? ArrivalTime { get; set; } // Added property for ArrivalTime
        public List<FlightVM> Flights { get; set; } = new List<FlightVM>();
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
        public int PassengerCount { get; set; } = 1; // Default to 1 passenger
        public double TotalPrice { get; set; }

        public double GetTotalPrice()
        {
            if (Passengers == null || !Passengers.Any())
            {
                // If no passengers with booking classes yet, use base calculation
                return Flights.Sum(f => f.Price ?? 0) * PassengerCount;
            }

            // Calculate total based on each passenger's booking class
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
