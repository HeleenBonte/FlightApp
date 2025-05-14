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
    public class TicketDAO : ITicketDAO
    {
        private readonly FlightsDbContext dbContext;

        public TicketDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task<int> GetCountByFlightIDAsync(int flightID)
        {
            try
            {
                return await dbContext.Tickets
                    .Where(t => t.FlightId == flightID)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }
            public async Task AddAsync(Ticket entity)
        {
            try
            {
                await dbContext.Tickets.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task DeleteAsync(Ticket entity)
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

        public async Task<Ticket?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Tickets
                    .Where(e => e.TicketId == Id)
                    .Include(f => f.Passenger)
                    .Include(f => f.BookingClass)
                    .Include(f => f.Flight)
                    .Include(f => f.MealChoice)
                    .Include(f => f.Flight.DepartureCityNavigation)
                    .Include(f => f.Flight.ArrivalCityNavigation)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Tickets
                    .Include(e => e.Passenger)
                    .Include(f => f.BookingClass)
                    .Include(f => f.Flight)
                    .Include(f => f.MealChoice)
                    .Include(f => f.Flight.DepartureCityNavigation)
                    .Include(f => f.Flight.ArrivalCityNavigation)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByBookingIdAsync(int bookingId)
        {
            try
            {
                return await dbContext.Tickets
                    .Where(t => t.BookingId == bookingId)
                    .Include(e => e.Passenger)
                    .Include(f => f.BookingClass)
                    .Include(f => f.Flight)
                    .Include(f => f.MealChoice)
                    .Include(f => f.Flight.DepartureCityNavigation)
                    .Include(f => f.Flight.ArrivalCityNavigation)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting tickets by booking ID: " + ex.Message);
                throw;
            }
        }

        public Task UpdateAsync(Ticket entity)
        {
            throw new NotImplementedException();
        }
    }
}
