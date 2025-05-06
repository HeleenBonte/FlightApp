using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories.Interfaces
{
    public interface IFlightDAO : IDAO<Flight>
    {
        Task<IEnumerable<Flight>> GetFlightsByCitiesID(int arrivalCityId, int departureCityId, DateOnly departureDate);

    }
}
