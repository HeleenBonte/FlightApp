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
    public class PassengerDAO : IPassengerDAO
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

        public async Task UpdateAsync(Passenger entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
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
        public async Task<Passenger?> FindIsExistingPassenger(string firstname, string lastname, string email)
        {
            try
            {
                return await dbContext.Passengers
                    .FirstOrDefaultAsync(p =>
                                p.FirstName.ToLower() == firstname.ToLower() &&
                              p.LastName.ToLower() == lastname.ToLower() &&
                                p.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
