using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class PassengerDAO : IDAO<Passenger>
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
