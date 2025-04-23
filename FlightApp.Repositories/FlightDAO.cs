using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class FlightDAO : IDAO<Flight>
    {
        public Task AddAsync(Flight entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Flight entity)
        {
            throw new NotImplementedException();
        }

        public Task<Flight?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Flight>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Flight entity)
        {
            throw new NotImplementedException();
        }
    }
}
