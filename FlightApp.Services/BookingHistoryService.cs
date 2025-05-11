using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interfaces;
using FlightApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class BookingHistoryService : IBookingHistoryService
    {
        private readonly IBookingHistoryDAO _bookingHistoryDAO;
        public BookingHistoryService(IBookingHistoryDAO bookingHistoryDAO)
        {
            _bookingHistoryDAO = bookingHistoryDAO;
        }
        public async Task AddAsync(BookingHistory entity)
        {
            await _bookingHistoryDAO.AddAsync(entity);
        }

        public async Task DeleteAsync(BookingHistory entity)
        {
            await _bookingHistoryDAO.DeleteAsync(entity);
        }

        public Task<BookingHistory?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BookingHistory>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<BookingHistory>> GetAllByUserIdAsync(string userId)
        {
            return await _bookingHistoryDAO.GetAllByUserIdAsync(userId);
        }
        public async Task<BookingHistory> GetAllByBookingIdAsync(int bookingId)
        {
            return await _bookingHistoryDAO.GetAllByBookingIdAsync(bookingId);
        }

        public Task UpdateAsync(BookingHistory entity)
        {
            throw new NotImplementedException();
        }
    }
}
