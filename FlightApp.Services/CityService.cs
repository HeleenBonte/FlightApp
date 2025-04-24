using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class CityService : IService<City>
    {
        private readonly IDAO<City> _cityDAO;

        public CityService(IDAO<City> cityDAO)
        {
            _cityDAO = cityDAO;
        }

        public async Task<IEnumerable<City>?> GetAllAsync()
        {
            return await _cityDAO.GetAllAsync();
        }

        public Task AddAsync(City entity) => throw new NotImplementedException();
        public Task DeleteAsync(City entity) => throw new NotImplementedException();
        public Task<City?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(City entity) => throw new NotImplementedException();
    }
}
