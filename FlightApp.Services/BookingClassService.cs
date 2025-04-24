using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class BookingClassService : IService<BookingClass>
    {
        private readonly IDAO<BookingClass> _bookingClassDAO;

        public BookingClassService(IDAO<BookingClass> bookingClassDAO)
        {
            _bookingClassDAO = bookingClassDAO;
        }

        public async Task<IEnumerable<BookingClass>?> GetAllAsync()
        {
            return await _bookingClassDAO.GetAllAsync();
        }

        public Task AddAsync(BookingClass entity) => throw new NotImplementedException();
        public Task DeleteAsync(BookingClass entity) => throw new NotImplementedException();
        public Task<BookingClass?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(BookingClass entity) => throw new NotImplementedException();
    }
}
