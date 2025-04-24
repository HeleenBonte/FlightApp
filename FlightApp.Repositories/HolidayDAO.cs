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
    public class HolidayDAO : IDAO<Holiday>
    {
        private readonly FlightsDbContext dbContext;

        public HolidayDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(Holiday entity)
        {
            try
            {
                await dbContext.Holidays.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(Holiday entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Holiday?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Holidays.Where(e => e.HolidayId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Holiday>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Holidays.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(Holiday entity)
        {
            throw new NotImplementedException();
        }
    }
}
