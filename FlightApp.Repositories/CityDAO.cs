using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class CityDAO : IDAO<City>
    {
        public Task AddAsync(City entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(City entity)
        {
            throw new NotImplementedException();
        }

        public Task<City?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<City>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(City entity)
        {
            throw new NotImplementedException();
        }
    }
}
