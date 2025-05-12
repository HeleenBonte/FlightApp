using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class PassengerService : IService<Passenger>
    {
        private readonly IDAO<Passenger> _passengerDAO;

        public PassengerService(IDAO<Passenger> passengerDAO)
        {
            _passengerDAO = passengerDAO;
        }

        public async Task<IEnumerable<Passenger>?> GetAllAsync()
        {
            return await _passengerDAO.GetAllAsync();
        }

        public Task AddAsync(Passenger entity) => throw new NotImplementedException();
        public Task DeleteAsync(Passenger entity) => throw new NotImplementedException();
        public async Task<Passenger?> FindByIdAsync(int id)
        {
            return await _passengerDAO.FindByIdAsync(id);
        }
        public Task UpdateAsync(Passenger entity) => throw new NotImplementedException();
    }
}
