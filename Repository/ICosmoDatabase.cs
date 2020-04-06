using API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Repository
{
    public interface ICosmoDatabase<T>
    {
        Task<IEnumerable<T>> GetItemsAsync(string query);
        Task<T> GetItemAsync(string id);
        Task AddItemAsync<U>(U item) where U : CosmoModel;
        Task UpdateItemAsync(string id, T item);
        Task DeleteItemAsync(string id);
    }
}
