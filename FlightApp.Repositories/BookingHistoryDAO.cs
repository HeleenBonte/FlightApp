using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class BookingHistoryDAO : IBookingHistoryDAO
    {
        private readonly FlightsDbContext dbContext;

        public BookingHistoryDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(BookingHistory entity)
        {
            await dbContext.BookingHistories.AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }

        public Task DeleteAsync(BookingHistory entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BookingHistory?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();

        }

        public Task<IEnumerable<BookingHistory>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<BookingHistory>?> GetAllByUserIdAsync(string userId)
        {
            return await dbContext.BookingHistories
                .Include(b => b.Booking)
                .Include(b => b.Booking.Route)
                .Include(b => b.Booking.Route.ArrivalCity)
                .Include(b => b.Booking.Route.DepartureCity)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public Task UpdateAsync(BookingHistory entity)
        {
            throw new NotImplementedException();
        }
    }
}
