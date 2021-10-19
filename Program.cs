using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GetInventoryApi;
using Microsoft.Azure.Cosmos;

namespace FifthTimesTheCharm
{
    public class Program
    {
        public static void Main()
        {
            var config = new ConfigurationBuilder()
                // .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();
            var cxnStr = config.GetConnectionString("Cosmos");
            var cosmosClient = new CosmosClient(cxnStr );
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                
                .ConfigureServices(svc =>
                    svc.AddSingleton<ICosmosDbService>(new CosmosDbService(cosmosClient, "Inventory", "Items"))
                )
                .Build();

            host.Run();
        }
    }
}