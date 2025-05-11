using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;

namespace FlightApp.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightDAO _flightDAO;

        public FlightService(IFlightDAO flightDAO)
        {
            _flightDAO = flightDAO;
        }

        public async Task<IEnumerable<Flight>?> GetAllAsync()
        {
            return await _flightDAO.GetAllAsync();
        }
        public async Task<IEnumerable<Flight>> GetFlightsByCitiesID(int arrivalCityId, int departureCityId, DateOnly departureDate)
        {
            return await _flightDAO.GetFlightsByCitiesID(arrivalCityId, departureCityId, departureDate);
        }

        public async Task<IEnumerable<Flight>> GetFlightsByCitiesID(int arrivalCityId, int departureCityId)
        {
            return await _flightDAO.GetFlightsByCitiesID(arrivalCityId, departureCityId);
        }

        public Task AddAsync(Flight entity) => throw new NotImplementedException();
        public Task DeleteAsync(Flight entity) => throw new NotImplementedException();
        public async Task<Flight?> FindByIdAsync(int id)
        {
            return await _flightDAO.FindByIdAsync(id);
        }
        public Task UpdateAsync(Flight entity) => throw new NotImplementedException();
    }
}
