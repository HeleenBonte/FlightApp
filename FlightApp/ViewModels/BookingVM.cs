namespace FlightApp.ViewModels
{
    public class BookingVM
    {
        public int BookingId { get; set; }
        public string? UserName { get; set; }
        public DateOnly BookingTime { get; set; }
        public bool PaymentStatus { get; set; }
        public int RouteId { get; set; }

    }
}
