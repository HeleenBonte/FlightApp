using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class HolidayDAO : IDAO<Holiday>
    {
        public Task AddAsync(Holiday entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Holiday entity)
        {
            throw new NotImplementedException();
        }

        public Task<Holiday?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Holiday>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Holiday entity)
        {
            throw new NotImplementedException();
        }
    }
}
