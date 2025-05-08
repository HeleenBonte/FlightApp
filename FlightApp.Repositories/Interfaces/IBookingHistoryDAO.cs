using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories.Interfaces
{
    public interface IBookingHistoryDAO : IDAO<BookingHistory>
    {
         Task<IEnumerable<BookingHistory>?> GetAllByUserIdAsync(string userId);
    }
}
