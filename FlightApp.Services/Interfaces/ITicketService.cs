using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services.Interfaces
{
    public interface ITicketService : IService<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsByBookingIdAsync(int bookingId);
        Task<int> GetCountByFlightIDAsync(int flightID);
    }
}