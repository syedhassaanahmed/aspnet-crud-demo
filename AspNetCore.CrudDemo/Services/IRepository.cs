using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.CrudDemo.Services
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(string id);
        Task<T> CreateAsync(T item);
        Task<bool> UpdateAsync(string id, T item);
        Task<bool> DeleteAsync(string id);
    }
}