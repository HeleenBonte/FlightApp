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
        public Task DeleteAsync(Booking entity) => throw new NotImplementedException();
        public Task<Booking?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(Booking entity) => throw new NotImplementedException();
    }
}
