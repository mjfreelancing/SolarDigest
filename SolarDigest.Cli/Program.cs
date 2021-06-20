using ConsoleService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SolarDigest.Cli.Commands.Download;
using SolarDigest.Cli.Commands.Upload;
using System.Threading.Tasks;

namespace SolarDigest.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ConsoleHost
                .Build(args, hostBuilder =>
                {
                    hostBuilder
                        .ConfigureAppConfiguration((hostContext, builder) =>
                        {
                            // Add other providers for JSON, etc.

                            if (hostContext.HostingEnvironment.IsDevelopment())
                            {
                                builder.AddUserSecrets<Program>();
                            }
                        })

                        //.UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                        
                        .ConfigureLogging((hostContext,logging) =>
                        {
                            logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                            logging.SetMinimumLevel(LogLevel.Debug);
                            logging.AddConsole();
                        })

                        .ConfigureServices((hostContext, services) =>
                        {
                            services
                                .AddSingleton<IConsoleApp, SolarDigestCli>();
                                //.AddOptions<MyConsoleAppSettings>().Bind(hostContext.Configuration.GetSection("ConsoleApp"))
                                //.PostConfigure(settings =>
                                //{
                                //    settings.SetDefaults();
                                //});

                                services.AddTransient<UploadFileCommand>();
                                services.AddTransient<DownloadFileCommand>();
                        });
                })
                .Run();
        }
    }
}
