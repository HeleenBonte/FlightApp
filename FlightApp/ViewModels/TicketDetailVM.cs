namespace FlightApp.ViewModels
{
    public class TicketDetailVM
    {
        public int TicketId { get; set; }
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int SeatNumber { get; set; }
        public string BookingClassName { get; set; }
        public string? Notes { get; set; }
    }
}
