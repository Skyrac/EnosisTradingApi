using API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Repository
{
    public interface ICosmosDatabase<T>
    {
        Task<IEnumerable<T>> GetItemsAsync(string query);
        Task<T> GetItemAsync(string id);
        Task AddItemAsync<U>(U item) where U : CosmoModel;
        Task UpdateItemAsync(string id, T item);
        Task DeleteItemAsync(string id);

        Task GetItemsAndCallMethodAsync(string queryString, Action<T> callback);
    }
}
