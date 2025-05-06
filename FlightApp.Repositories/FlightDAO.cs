using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class FlightDAO : IFlightDAO
    {
        private readonly FlightsDbContext dbContext;

        public FlightDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(Flight entity)
        {
            try
            {
                await dbContext.Flights.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(Flight entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Flight?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Flights
                    .Include(f => f.ArrivalCityNavigation)
                    .Include(f => f.DepartureCityNavigation)
                    .Where(f => f.FlightId == Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Flight>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Flights
                    .Include(f => f.ArrivalCityNavigation)
                    .Include(f => f.DepartureCityNavigation)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(Flight entity)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Flight>> GetFlightsByCitiesID(int arrivalCityId, int departureCityId, DateOnly departureDate)
        {
            try
            {
                return await dbContext.Flights
                    .Include(f => f.ArrivalCityNavigation)
                    .Include(f => f.DepartureCityNavigation)
                    .Where(f => f.ArrivalCity == arrivalCityId && f.DepartureCity == departureCityId && f.DepartureTime >= departureDate.ToDateTime(TimeOnly.MinValue))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }
    }
}
