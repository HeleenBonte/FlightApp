using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightApp.Repositories.Interfaces;

namespace FlightApp.Services
{
    public class PassengerService : IPassengerService
    {
        private readonly IPassengerDAO _passengerDAO;

        public PassengerService(IPassengerDAO passengerDAO)
        {
            _passengerDAO = passengerDAO;
        }

        public async Task<IEnumerable<Passenger>?> GetAllAsync()
        {
            return await _passengerDAO.GetAllAsync();
        }

        public async Task AddAsync(Passenger entity) {
        await _passengerDAO.AddAsync(entity);
        }
        public Task DeleteAsync(Passenger entity) => throw new NotImplementedException();
        public async Task<Passenger?> FindByIdAsync(int id)
        {
            return await _passengerDAO.FindByIdAsync(id);
        }
        public async Task UpdateAsync(Passenger entity){
            await _passengerDAO.UpdateAsync(entity);
        }
        public async Task<Passenger?> FindIsExistingPassenger(string firstname, string lastname, string email)
        {
            return await _passengerDAO.FindIsExistingPassenger(firstname, lastname, email);
        }
    }
}
