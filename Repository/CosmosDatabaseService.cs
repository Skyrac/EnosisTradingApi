using API.Models;
using API.Utility;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace API.Repository
{
    public class CosmosDatabaseService<T> : ICosmosDatabase<T>
    {
        private Container _container;
        public CosmosDatabaseService(CosmosClient dbClient, string databaseName, string containerName)
        {
            _container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync<U>(U item) where U : CosmoModel
        {
            item.id = IdGenerator.GenerateId(typeof(U).Name);
            await _container.CreateItemAsync(item, new PartitionKey(item.id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
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
                return default;
            }

        }

        public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task GetItemsAndCallMethodAsync(string queryString, Action<T> callback)
        {
            var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                foreach(var item in response)
                {
                    callback.Invoke(item);
                }
            }
        }

        public async Task UpdateItemAsync(string id, T item)
        {
            await _container.UpsertItemAsync(item, new PartitionKey(id));
        }

        public async Task<BulkOperationResponse<U>> BulkUpdateAsync<U>(IEnumerable<U> items) where U : CosmoModel, T
        {
            List<Task<OperationsResponse<U>>> operations = new List<Task<OperationsResponse<U>>>(items.Count());
            foreach (var document in items)
            {
                operations.Add(CaptureOperationResponse(_container.ReplaceItemAsync(document, document.id, new PartitionKey(document.id)), document));
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            await Task.WhenAll(operations);
            stopwatch.Stop();

            var bulkOperationResponse = new BulkOperationResponse<U>()
            {
                TotalTimeTaken = stopwatch.Elapsed,
                TotalRequestUnitsConsumed = operations.Sum(task => task.Result.RequestUnitsConsumed),
                SuccessfulDocuments = operations.Count(task => task.Result.IsSuccessful),
                Failures = operations.Where(task => !task.Result.IsSuccessful).Select(task => (task.Result.Item, task.Result.CosmosException)).ToList()
            };

            return bulkOperationResponse;
        }

        public static Task<OperationsResponse<U>> CaptureOperationResponse<U>(Task<ItemResponse<U>> task, U item)
        {
            return task.ContinueWith(itemResponse =>
            {
                if (itemResponse.IsCompletedSuccessfully)
                {
                    return new OperationsResponse<U>()
                    {
                        Item = item,
                        IsSuccessful = true,
                        RequestUnitsConsumed = task.Result.RequestCharge
                    };
                }


                AggregateException innerExceptions = itemResponse.Exception.Flatten();
                CosmosException cosmosException = innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) as CosmosException;
                if (cosmosException != null)
                {
                    return new OperationsResponse<U>()
                    {
                        Item = item,
                        RequestUnitsConsumed = cosmosException.RequestCharge,
                        IsSuccessful = false,
                        CosmosException = cosmosException
                    };
                }

                return new OperationsResponse<U>()
                {
                    Item = item,
                    IsSuccessful = false,
                    CosmosException = innerExceptions.InnerExceptions.FirstOrDefault()
                };
            });
        }




    }
}

