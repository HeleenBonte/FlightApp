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
    }
}
