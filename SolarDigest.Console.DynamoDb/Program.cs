using ConsoleService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SolarDigest.Console.DynamoDb
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ConsoleHost
                .Build(args, hostBuilder =>
                {
                    hostBuilder
                        .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                        .ConfigureLogging(logging =>
                        {
                            logging.AddConsole();
                        })
                        .ConfigureServices((hostContext, services) =>
                        {
                            services
                                .AddSingleton<IConsoleApp, DynamoDbConsole>();
                                //.AddOptions<MyConsoleAppSettings>().Bind(hostContext.Configuration.GetSection("ConsoleApp"))
                                //.PostConfigure(settings =>
                                //{
                                //    settings.SetDefaults();
                                //});
                        });
                })
                .Run();
        }
    }
}
