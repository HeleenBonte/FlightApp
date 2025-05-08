using System;
using System.Collections.Generic;

namespace FlightApp.ViewModels
{
    public class ConfirmBookingVM
    {
        // Booking fields
        public int BookingId { get; set; }
        public string UserId { get; set; }
        public DateTime BookingTime { get; set; } = DateTime.Now;
        public bool PaymentStatus { get; set; }
        public int RouteId { get; set; }

        // Navigation properties
        public List<TicketVM> Tickets { get; set; } = new List<TicketVM>();
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string Layover1 { get; set; }
        public string Layover2 { get; set; }

        // Payment information
        public double TotalPrice { get; set; }

        // Passenger information
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
    }
}