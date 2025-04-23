using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class CityService : IService<City>
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
