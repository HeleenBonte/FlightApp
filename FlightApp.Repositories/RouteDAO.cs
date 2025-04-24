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
    public class RouteDAO : IDAO<Route>
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
                return await dbContext.Routes.Where(e => e.RouteId == Id).FirstOrDefaultAsync();
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
                return await dbContext.Routes.ToListAsync();
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
