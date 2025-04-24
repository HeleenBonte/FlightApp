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
    public class TicketDAO : IDAO<Ticket>
    {
        private readonly FlightsDbContext dbContext;

        public TicketDAO(FlightsDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task AddAsync(Ticket entity)
        {
            try
            {
                await dbContext.Tickets.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public Task DeleteAsync(Ticket entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Ticket?> FindByIdAsync(int Id)
        {
            try
            {
                return await dbContext.Tickets.Where(e => e.TicketId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error DAO");
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>?> GetAllAsync()
        {
            try
            {
                return await dbContext.Tickets.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in DAO");
                throw;
            }
        }

        public Task UpdateAsync(Ticket entity)
        {
            throw new NotImplementedException();
        }
    }
}
