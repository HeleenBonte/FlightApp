using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class RouteService : IService<Route>
    {
        private readonly IDAO<Route> _routeDAO;

        public RouteService(IDAO<Route> routeDAO)
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
    }
}
