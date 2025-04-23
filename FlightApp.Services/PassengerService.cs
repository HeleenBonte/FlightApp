using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class PassengerService : IService<Passenger>
    {
        public Task AddAsync(Passenger entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Passenger entity)
        {
            throw new NotImplementedException();
        }

        public Task<Passenger?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Passenger>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Passenger entity)
        {
            throw new NotImplementedException();
        }
    }
}
