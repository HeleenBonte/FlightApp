namespace FlightApp.ViewModels
{
    public class TicketVM
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public int BookingClassId { get; set; }
        public int PassengerId { get; set; }
        public int SeatNumber { get; set; }
        public int MealChoiceId { get; set; }
        public int BookingId { get; set; }
        public string? PassengerName { get; set; }
        public string? FlightDeparture { get; set; }
        public string? FlightArrival { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? BookingClassName { get; set; }
        public string? MealChoiceType { get; set; }
        public string? Notes { get; set; }
    }
}