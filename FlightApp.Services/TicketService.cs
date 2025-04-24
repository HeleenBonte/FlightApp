using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class TicketService : IService<Ticket>
    {
        private readonly IDAO<Ticket> _ticketDAO;

        public TicketService(IDAO<Ticket> ticketDAO)
        {
            _ticketDAO = ticketDAO;
        }

        public async Task<IEnumerable<Ticket>?> GetAllAsync()
        {
            return await _ticketDAO.GetAllAsync();
        }

        public Task AddAsync(Ticket entity) => throw new NotImplementedException();
        public Task DeleteAsync(Ticket entity) => throw new NotImplementedException();
        public Task<Ticket?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(Ticket entity) => throw new NotImplementedException();
    }
}
