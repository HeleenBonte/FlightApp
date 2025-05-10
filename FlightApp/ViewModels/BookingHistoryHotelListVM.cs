namespace FlightApp.ViewModels
{
    public class BookingHistoryHotelListVM
    {
        public List<BookingHistoryVM> BookingHistory { get; set; }
        public List<HotelVM> Hotels { get; set; } = new List<HotelVM>();
    }
}
