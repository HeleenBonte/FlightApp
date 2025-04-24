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
    public class MealChoiceDAO : IDAO<MealChoice>
    {
        private readonly FlightsDbContext dbContext;

        public MealChoiceDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(MealChoice entity)
        {
            try
            {
                await dbContext.MealChoices.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(MealChoice entity)
        {
            throw new NotImplementedException();
        }

        public async Task<MealChoice?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.MealChoices.Where(e => e.MealChoiceId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<MealChoice>?> GetAllAsync()
        {
            try
            {
                return await dbContext.MealChoices.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(MealChoice entity)
        {
            throw new NotImplementedException();
        }
    }
}
