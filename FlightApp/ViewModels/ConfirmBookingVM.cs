using System;
using System.Collections.Generic;

namespace FlightApp.ViewModels
{
    public class ConfirmBookingVM
    {
        // Booking fields
        public int BookingId { get; set; }
        public string? UserId { get; set; }
        public DateTime BookingTime { get; set; } = DateTime.Now;
        public bool PaymentStatus { get; set; }
        public int? RouteId { get; set; }

        // Navigation properties
        public List<TicketVM> Tickets { get; set; } = new List<TicketVM>();
        public string? DepartureCity { get; set; }
        public string? ArrivalCity { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? Layover1 { get; set; }
        public string? Layover2 { get; set; }

        // Support for multiple routes
        public List<RouteViewModel> Routes { get; set; } = new List<RouteViewModel>();

        // Payment information
        public double TotalPrice { get; set; }

        // Passenger information
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
    }

    // New class to support multiple routes
    public class RouteViewModel
    {
        public int? RouteId { get; set; }
        public string? DepartureCity { get; set; }
        public string? ArrivalCity { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? Layover1 { get; set; }
        public string? Layover2 { get; set; }
        public double Price { get; set; }
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
        public List<FlightViewModel> Flights { get; set; } = new List<FlightViewModel>();
        public List<TicketVM> Tickets { get; set; } = new List<TicketVM>();
    }

    public class FlightViewModel
    {
        public int FlightId { get; set; }
        public string? DepartureCity { get; set; }
        public string? ArrivalCity { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public double Price { get; set; }
    }
}
