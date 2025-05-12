using FlightApp.Domains.EntitiesDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services.Interfaces
{
    public interface IPassengerService : IService<Passenger>
    {
        Task<Passenger?> FindIsExistingPassenger(string firstname, string lastname, string email);
    }
}
