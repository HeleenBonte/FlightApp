using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class RouteDAO : IDAO<Route>
    {
        public Task AddAsync(Route entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Route entity)
        {
            throw new NotImplementedException();
        }

        public Task<Route?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Route>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Route entity)
        {
            throw new NotImplementedException();
        }
    }
}
