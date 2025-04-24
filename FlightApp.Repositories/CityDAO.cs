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
    public class CityDAO : IDAO<City>
    {
        private readonly FlightsDbContext dbContext;

        public CityDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(City entity)
        {
            try
            {
                await dbContext.Cities.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(City entity)
        {
            throw new NotImplementedException();
        }

        public async Task<City?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Cities.Where(c => c.CityId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<City>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Cities.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(City entity)
        {
            throw new NotImplementedException();
        }
    }
}
