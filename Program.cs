using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace GetInventoryApi
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(cfg =>
                    cfg.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                )
                .ConfigureFunctionsWorkerDefaults()
                
                .ConfigureServices(svc =>
                    svc
                        .AddSingleton<ICosmosDbService, CosmosDbService>()
                        .AddLogging()
                )
                .Build();

            await host.RunAsync();
        }
    }
}