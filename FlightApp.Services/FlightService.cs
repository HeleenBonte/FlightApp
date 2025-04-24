using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class FlightService : IService<Flight>
    {
        private readonly IDAO<Flight> _flightDAO;

        public FlightService(IDAO<Flight> flightDAO)
        {
            _flightDAO = flightDAO;
        }

        public async Task<IEnumerable<Flight>?> GetAllAsync()
        {
            return await _flightDAO.GetAllAsync();
        }

        public Task AddAsync(Flight entity) => throw new NotImplementedException();
        public Task DeleteAsync(Flight entity) => throw new NotImplementedException();
        public Task<Flight?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(Flight entity) => throw new NotImplementedException();
    }
}
