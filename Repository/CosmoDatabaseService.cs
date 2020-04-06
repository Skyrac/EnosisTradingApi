using API.Models;
using API.Utility;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Repository
{
    public class CosmoDatabaseService<T> : ICosmoDatabase<T>
    {
        private Container _container;

        public CosmoDatabaseService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync<U>(U item) where U : CosmoModel
        {
            item.id = IdGenerator.GenerateId(typeof(U).Name);
            await this._container.CreateItemAsync(item);
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<T>(id, new PartitionKey(id));
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<T> response = await this._container.ReadItemAsync<T>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default(T);
            }

        }

        public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, T item)
        {
            await _container.UpsertItemAsync<T>(item, new PartitionKey(id));
        }
    }
}

