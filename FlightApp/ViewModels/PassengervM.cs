namespace FlightApp.ViewModels
{
    public class PassengerVM
    {
        public int PassengerId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int FlightId { get; set; }
        public int SeatNumber { get; set; }
        public bool IsCheckedIn { get; set; } = false;
        public int MealChoiceId { get; set; }
        public int BookingClassId { get; set; } = 1; 
        public string? BookingClassName { get; set; }
        public double BookingClassPriceFactor { get; set; } = 1.0; 

        public List<TicketDetailVM> TicketDetails { get; set; } = new List<TicketDetailVM>();
    }
}
