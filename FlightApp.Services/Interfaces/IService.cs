using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Services.Interfaces
{
    public interface IService<T> where T : class
    {
        Task<IEnumerable<T>?> GetAllAsync();
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        Task UpdateAsync(T entity);
        Task<T?> FindByIdAsync(int Id);
    }
}
