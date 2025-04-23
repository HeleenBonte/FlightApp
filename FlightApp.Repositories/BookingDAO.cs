using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class BookingDAO : IDAO<Booking>
    {
        public Task AddAsync(Booking entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Booking entity)
        {
            throw new NotImplementedException();
        }

        public Task<Booking?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Booking>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Booking entity)
        {
            throw new NotImplementedException();
        }
    }
}
