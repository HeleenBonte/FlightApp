using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using FlightApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class AspUserService : IService<AspNetUser>
    {
        private readonly IDAO<AspNetUser> _aspUserDAO;

        public AspUserService(IDAO<AspNetUser> aspUserDAO)
        {
            _aspUserDAO = aspUserDAO;
        }   

        public Task AddAsync(AspNetUser entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(AspNetUser entity)
        {
            throw new NotImplementedException();
        }

        public Task<AspNetUser?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AspNetUser>?> GetAllAsync()
        {
            return await _aspUserDAO.GetAllAsync();
        }

        public Task UpdateAsync(AspNetUser entity)
        {
            throw new NotImplementedException();
        }
    }
}
