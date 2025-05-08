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
    }
}
