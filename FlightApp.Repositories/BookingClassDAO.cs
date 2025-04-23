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
    public class BookingClassDAO : IDAO<BookingClass>
    {
        public readonly FlightsDbContext dbContext;
        public BookingClassDAO(FlightsDbContext _dbContext)
        {
            this.dbContext = _dbContext;
        }

        public async Task AddAsync(BookingClass entity)
        {
            try
            {
                await dbContext.BookingClasses.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(BookingClass entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BookingClass?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.BookingClasses.Where(e => e.BookingClassId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<BookingClass>?> GetAllAsync()
        {
            try
            {
                return await dbContext.BookingClasses.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(BookingClass entity)
        {
            throw new NotImplementedException();
        }
    }
}
