using FlightApp.Domains.EntityAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Util.Hotels.Interfaces
{
    public interface IHotelService
    {
        Task<List<HotelId>?> GetHotelIdsAsync(string cityApiId);
        Task<Hotel?> GetHotelByIdAsync(int hotelApiId);
    }
}
