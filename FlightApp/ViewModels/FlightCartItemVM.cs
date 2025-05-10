// FlightApp/ViewModels/FlightCartItemVM.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightApp.ViewModels
{
    public class FlightCartItemVM
    {
        public int FlightId { get; set; }
        public DateTime? DepartureTime { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public double Price { get; set; }
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
        public int PassengerCount { get; set; } = 1; // Default to 1 passenger
        public double TotalPrice { get; set; }

        public double GetTotalPrice()
        {
            if (Passengers == null || !Passengers.Any())
            {
                // If no passengers with booking classes yet, use base calculation
                return Price * PassengerCount;
            }

            // Calculate total based on each passenger's booking class
            double total = 0;
            foreach (var passenger in Passengers)
            {
                total += Price * passenger.BookingClassPriceFactor;
            }

            return total;
        }
    }
}