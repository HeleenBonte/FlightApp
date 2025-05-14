using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using FlightApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class RouteDAO : IRouteDAO
    {
        private readonly FlightsDbContext dbContext;

        public RouteDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(Route entity)
        {
            try
            {
                await dbContext.Routes.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(Route entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Route?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Routes.Include(e => e.ArrivalCity).Include(e => e.DepartureCity).Include(e => e.Flights).Where(e => e.RouteId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Route>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Routes.Include(x => x.Flights).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Route>> GetRoutesByCitiesID(int arrivalCityId, int departureCityId, DateOnly departureDate)
        {
            try
            {
                return await dbContext.Routes
                    .Include(f => f.ArrivalCity)
                    .Include(f => f.DepartureCity)
                    .Where(f => f.ArrivalCityId == arrivalCityId && f.DepartureCityId == departureCityId && f.DepartureTime >= departureDate.ToDateTime(TimeOnly.MinValue) && f.DepartureTime <= departureDate.ToDateTime(TimeOnly.MaxValue))
                    .Include(f => f.Flights)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(Route entity)
        {
            throw new NotImplementedException();
        }
    }
}
