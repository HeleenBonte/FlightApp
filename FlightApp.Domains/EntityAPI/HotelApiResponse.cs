using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Domains.EntityAPI
{
    public class HotelApiResponse
    {
        public bool status { get; set; }
        public Hotel Data { get; set; }
    }
    public class Hotel
    {
        public int hotel_id { get; set; }
        public string Url { get; set; }
        public string Hotel_name { get; set; }
        public PriceClass composite_price_breakdown { get; set; }
       public RawData rawData { get; set; }
    }
    public class RawData
    {
        public List<string> photoUrls { get; set; }

    }
    public class PriceClass
    {
        public Price all_inclusive_amount { get; set; }
    }
    public class Price
    {
        public double value { get; set; }
        public string amount_rounded { get; set; }
    }
}
