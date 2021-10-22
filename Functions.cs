using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GetInventoryApi.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetInventoryApi
{
    public class Functions
    {
        private readonly ICosmosDbService dbService;

        public Functions(ICosmosDbService dbService)
        {
            this.dbService = dbService;
        }
        
        [Function("getInventory")]
        public async Task<HttpResponseData> GetInventory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Functions");
            logger.LogInformation("Getting all inventory.");

            var results = await dbService.GetItemsAsync("SELECT * FROM c");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(results);

            return response;
        }

        [Function("addItem")]
        public async Task<HttpResponseData> AddItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Functions");
            logger.LogInformation("Adding new item to inventory.");

            var itemJson = await new StreamReader(req.Body).ReadToEndAsync();
            var item = JsonConvert.DeserializeObject<Item>(itemJson);
            item.Id = Guid.NewGuid().ToString();

            await dbService.AddItemAsync(item);

            var response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }

        
        [Function("editItem")]
        public async Task<HttpResponseData> EditItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Functions");
            logger.LogInformation("Updating item in inventory.");

            var itemJson = await new StreamReader(req.Body).ReadToEndAsync();
            var item = JsonConvert.DeserializeObject<Item>(itemJson);

            await dbService.UpdateItemAsync(item);

            var response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }

        [Function("deleteItem")]
        public async Task<HttpResponseData> DeleteItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "deleteItem/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Functions");
            logger.LogInformation("Deleting item from inventory.");

            await dbService.DeleteItemAsync(id);

            var response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }
    }
}
