using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories
{
    public class AspUserDAO : IDAO<AspNetUser>
    {
        private readonly FlightsDbContext dbContext;

        public AspUserDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }
        public Task AddAsync(AspNetUser entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(AspNetUser entity)
        {
            throw new NotImplementedException();
        }

        public Task<AspNetUser?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AspNetUser>?> GetAllAsync()
        {
            try
            {
                return await dbContext.AspNetUsers.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(AspNetUser entity)
        {
            throw new NotImplementedException();
        }
    }
}
