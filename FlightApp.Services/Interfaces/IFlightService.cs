using FlightApp.Domains.EntitiesDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services.Interfaces
{
    public interface IFlightService : IService<Flight>
    {
        Task<IEnumerable<Flight>> GetFlightsByCitiesID(int arrivalCityId, int departureCityId);

    }
}
