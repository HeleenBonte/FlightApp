using FlightApp.Domains.EntitiesDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services.Interfaces
{
    public interface IRouteService : IService<Route>
    {
        Task<IEnumerable<Route>> GetRoutesByCitiesID(int arrivalCityId, int departureCityId);

    }
}
