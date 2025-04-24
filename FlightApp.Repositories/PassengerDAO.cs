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
    public class PassengerDAO : IDAO<Passenger>
    {
        private readonly FlightsDbContext dbContext;

        public PassengerDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(Passenger entity)
        {
            try
            {
                await dbContext.Passengers.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(Passenger entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Passenger?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Passengers.Where(e => e.PassengerId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Passenger>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Passengers.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(Passenger entity)
        {
            throw new NotImplementedException();
        }
    }
}
