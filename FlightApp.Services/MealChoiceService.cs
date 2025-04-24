using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Repositories.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class MealChoiceService : IService<MealChoice>
    {
        private readonly IDAO<MealChoice> _mealChoiceDAO;

        public MealChoiceService(IDAO<MealChoice> mealChoiceDAO)
        {
            _mealChoiceDAO = mealChoiceDAO;
        }

        public async Task<IEnumerable<MealChoice>?> GetAllAsync()
        {
            return await _mealChoiceDAO.GetAllAsync();
        }

        public Task AddAsync(MealChoice entity) => throw new NotImplementedException();
        public Task DeleteAsync(MealChoice entity) => throw new NotImplementedException();
        public Task<MealChoice?> FindByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateAsync(MealChoice entity) => throw new NotImplementedException();
    }
}
