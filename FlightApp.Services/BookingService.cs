using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class BookingService : IService<Booking>
    {
        private readonly IDAO<Booking> _bookingDAO;

        public BookingService(IDAO<Booking> bookingDAO)
        {
            _bookingDAO = bookingDAO;
        }

        public async Task<IEnumerable<Booking>?> GetAllAsync()
        {
            return await _bookingDAO.GetAllAsync();
        }

        public Task AddAsync(Booking entity) => throw new NotImplementedException();
        public async Task DeleteAsync(Booking entity) {
          await _bookingDAO.DeleteAsync(entity);
        }
        public async Task<Booking?> FindByIdAsync(int id) {
        return await _bookingDAO.FindByIdAsync(id);
        }
        public Task UpdateAsync(Booking entity) => throw new NotImplementedException();
    }
}
