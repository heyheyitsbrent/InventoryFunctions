using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetInventoryApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetInventoryApi
{
    public interface ICosmosDbService
    {
        Task AddItemAsync(Item item);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(string id);
        Task<IEnumerable<Item>> GetItemsAsync(string queryString);
        Task<Item> GetItemAsync(string id);
    }

    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(IConfiguration configuration)
        {
            var connectionStr = configuration.GetConnectionString("Cosmos");
            var logger = LoggerFactory.Create(bldr => bldr.AddConsole()).CreateLogger<CosmosDbService>();
            logger.LogInformation("Creating client with string: " + connectionStr);
            var databaseName = "Inventory";
            var containerName = "Items";
            var dbClient = new CosmosClient(connectionStr);
            this._container = dbClient.GetContainer(databaseName, containerName);
            logger.LogInformation("Created");
        }

        public async Task AddItemAsync(Item item) => await this._container.CreateItemAsync<Item>(item, new PartitionKey(item.Id));

        public async Task UpdateItemAsync(Item item) => await this._container.UpsertItemAsync<Item>(item, new PartitionKey(item.Id));

        public async Task DeleteItemAsync(string id) => await this._container.DeleteItemAsync<Item>(id, new PartitionKey(id));

        public async Task<IEnumerable<Item>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Item>(new QueryDefinition(queryString));
            List<Item> results = new List<Item>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<Item> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<Item> response = await this._container.ReadItemAsync<Item>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { 
                return null;
            }

        }


    }
}