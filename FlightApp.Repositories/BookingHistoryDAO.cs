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
        private readonly FlightsDbContext _dbContext;

        public BookingHistoryDAO(FlightsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(BookingHistory entity)
        {
            try
            {
                await _dbContext.BookingHistories.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(BookingHistory entity)
        {
            throw new NotImplementedException();
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
            return await _dbContext.BookingHistories
                .Where(bh => bh.UserId == userId)
                .Include(bh => bh.Booking)
                    .ThenInclude(b => b.Route)
                        .ThenInclude(r => r.DepartureCity)
                .Include(bh => bh.Booking)
                    .ThenInclude(b => b.Route)
                        .ThenInclude(r => r.ArrivalCity)
                .Include(bh => bh.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.DepartureCityNavigation)
                .Include(bh => bh.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.ArrivalCityNavigation)
                .Include(bh => bh.Booking.Passengers)
                .OrderByDescending(bh => bh.Booking.BookingTime)
                .ToListAsync();
        }

        public Task UpdateAsync(BookingHistory entity)
        {
            throw new NotImplementedException();
        }
    }
}
