using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Domains.EntityAPI
{
    public class HotelIDApiResponse
    {
        public bool Status { get; set; }
        public Data Data { get; set; }
        

    }
    public class HotelId
    {
        public int hotel_id { get; set; }
    }
   
    public class Data
    {
        public List<HotelId> Hotels { get; set; }
        public List<Meta> Meta { get; set; }
    }
    public class Meta
    {
        public string Title { get; set; }
    }

} 


  