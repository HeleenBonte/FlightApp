using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class MealChoiceService : IService<MealChoice>
    {
        public Task AddAsync(MealChoice entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(MealChoice entity)
        {
            throw new NotImplementedException();
        }

        public Task<MealChoice?> FindByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MealChoice>?> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(MealChoice entity)
        {
            throw new NotImplementedException();
        }
    }
}
