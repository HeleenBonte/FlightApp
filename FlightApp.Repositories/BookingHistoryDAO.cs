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
            try
            {
                await dbContext.BookingHistories.AddAsync(entity);
                await dbContext.SaveChangesAsync();
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
            try
            {
                return await dbContext.BookingHistories
            .Include(b => b.Booking)
            .Include(b => b.Booking.Route)
            .Include(b => b.Booking.Route.ArrivalCity)
            .Include(b => b.Booking.Route.DepartureCity)
            .Where(b => b.UserId == userId)
            .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public Task UpdateAsync(BookingHistory entity)
        {
            throw new NotImplementedException();
        }
    }
}
