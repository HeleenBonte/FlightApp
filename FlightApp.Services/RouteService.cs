using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightApp.Repositories;

namespace FlightApp.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteDAO _routeDAO;

        public RouteService(IRouteDAO routeDAO)
        {
            _routeDAO = routeDAO;
        }

        public async Task<IEnumerable<Route>?> GetAllAsync()
        {
            return await _routeDAO.GetAllAsync();
        }

        public Task AddAsync(Route entity) => throw new NotImplementedException();
        public Task DeleteAsync(Route entity) => throw new NotImplementedException();
        public Task<Route?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(Route entity) => throw new NotImplementedException();

        public async Task<IEnumerable<Route>> GetRoutesByCitiesID(int arrivalCityId, int departureCityId, DateOnly departureDate)
        {
            return await _routeDAO.GetRoutesByCitiesID(arrivalCityId, departureCityId, departureDate);
        }
    }
}
