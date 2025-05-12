using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Repositories.Interfaces
{
    public interface IPassengerDAO : IDAO<Passenger>
    {
        Task<Passenger?> FindIsExistingPassenger(string firstname, string lastname, string email);
    }
}
