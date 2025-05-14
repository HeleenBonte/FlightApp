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

        public async Task AddAsync(Booking entity)
        {
           await _bookingDAO.AddAsync(entity);

        }
        public async Task DeleteAsync(Booking entity) {
          await _bookingDAO.DeleteAsync(entity);
        }
        public async Task<Booking?> FindByIdAsync(int id) {
        return await _bookingDAO.FindByIdAsync(id);
        }
        public async Task UpdateAsync(Booking entity){
            await _bookingDAO.UpdateAsync(entity);
        }
    }
}
