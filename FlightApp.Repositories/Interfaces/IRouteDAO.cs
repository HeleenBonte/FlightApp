using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories.Interfaces
{
    public interface IRouteDAO : IDAO<Route>
    {
        Task<IEnumerable<Route>> GetRoutesByCitiesID(int arrivalCityId, int departureCityId);

    }
}
