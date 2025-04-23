using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class BookingService : IService<Booking>
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
