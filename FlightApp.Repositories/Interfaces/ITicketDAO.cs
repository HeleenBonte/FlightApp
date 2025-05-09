// FlightApp.Repositories/Interface/ITicketDAO.cs
using FlightApp.Domains.EntitiesDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Repositories.Interface
{
    public interface ITicketDAO : IDAO<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsByBookingIdAsync(int bookingId);
    }
}