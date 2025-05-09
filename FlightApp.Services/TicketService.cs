using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketDAO _ticketDAO;

        public TicketService(ITicketDAO ticketDAO)
        {
            _ticketDAO = ticketDAO;
        }

        public async Task<IEnumerable<Ticket>?> GetAllAsync()
        {
            return await _ticketDAO.GetAllAsync();
        }

        public Task AddAsync(Ticket entity) => throw new NotImplementedException();
        public Task DeleteAsync(Ticket entity) => throw new NotImplementedException();
        public async Task<Ticket?> FindByIdAsync(int id)
        {
           return await _ticketDAO.FindByIdAsync(id);
        }
        public Task UpdateAsync(Ticket entity) => throw new NotImplementedException();

        public async Task<IEnumerable<Ticket>> GetTicketsByBookingIdAsync(int bookingId)
        {
            try
            {
                return await _ticketDAO.GetTicketsByBookingIdAsync(bookingId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in service: {ex.Message}");
                throw;
            }
        }
    }
}
