using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class HolidayService : IService<Holiday>
    {
        private readonly IDAO<Holiday> _holidayDAO;

        public HolidayService(IDAO<Holiday> holidayDAO)
        {
            _holidayDAO = holidayDAO;
        }

        public async Task<IEnumerable<Holiday>?> GetAllAsync()
        {
            return await _holidayDAO.GetAllAsync();
        }

        public Task AddAsync(Holiday entity) => throw new NotImplementedException();
        public Task DeleteAsync(Holiday entity) => throw new NotImplementedException();
        public Task<Holiday?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(Holiday entity) => throw new NotImplementedException();
    }
}
