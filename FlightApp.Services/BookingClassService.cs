using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class BookingClassService : IService<BookingClass>
    {
        public Task AddAsync(BookingClass entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(BookingClass entity)
        {
            throw new NotImplementedException();
        }

        public Task<BookingClass?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BookingClass>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(BookingClass entity)
        {
            throw new NotImplementedException();
        }
    }
}
