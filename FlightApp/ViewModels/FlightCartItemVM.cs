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
        public int PassengerCount { get; set; } = 1; 
        public double TotalPrice { get; set; }
        public string? Notes { get; set; } 
        public bool IsComplete { get; set; } = false;
        public DateTime AddedToCartTime { get; set; } = DateTime.Now;

        public double GetTotalPrice()
        {
            if (Passengers == null || !Passengers.Any())
            {
                return Price * PassengerCount;
            }

            double total = 0;
            foreach (var passenger in Passengers)
            {
                double basePrice = Price;
                total += basePrice * passenger.BookingClassPriceFactor;
            }

            return total;
        }
    }
}
