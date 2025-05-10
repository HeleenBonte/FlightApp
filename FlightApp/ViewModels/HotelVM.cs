using FlightApp.Domains.EntityAPI;

namespace FlightApp.ViewModels
{
    public class HotelVM
    {
        public string Hotel_name { get; set; }
        public string Url { get; set; }
        public double Price { get; set; }
        public string PriceString { get; set; }
        public List<string> PhotoUrls { get; set; }
        public double ReviewScore { get; set; }
    }
}
