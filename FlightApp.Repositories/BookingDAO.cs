using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class BookingDAO : IDAO<Booking>
    {
        private readonly FlightsDbContext dbContext;

        public BookingDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(Booking entity)
        {
            try
            {
                await dbContext.Bookings.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task DeleteAsync(Booking entity)
        {
            dbContext.Entry(entity).State = EntityState.Deleted;
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task<Booking?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Bookings
                    .Include(b => b.Route)
                    .Include(b => b.BookingHistories)
                    .Where(b => b.BookingId == Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Booking>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Bookings
                    .Include(b => b.Route)
                    .Include(b => b.BookingHistories)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(Booking entity)
        {
            throw new NotImplementedException();
        }
    }
}
