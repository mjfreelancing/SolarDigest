using ConsoleService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SolarDigest.Cli.Commands.Download;
using SolarDigest.Cli.Commands.Power;
using SolarDigest.Cli.Commands.Site;
using SolarDigest.Cli.Commands.Upload;
using SolarDigest.Graphql;
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

                        .ConfigureLogging((hostContext, logging) =>
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

                            var graphqlConfig = new SolarDigestGraphqlConfiguration
                            {
                                // via user secrets / environment variables
                                ApiUrl = hostContext.Configuration.GetValue<string>("GraphqlUrl"),
                                ApiKey = hostContext.Configuration.GetValue<string>("x-api-key")
                            };

                            services.AddSingleton<ISolarDigestGraphqlConfiguration, SolarDigestGraphqlConfiguration>(_ => graphqlConfig);
                            services.AddTransient<ISolarDigestGraphql, SolarDigestGraphql>();
                            services.AddTransient<SitePowerCommand>();
                            services.AddTransient<SiteDetailsCommand>();
                            services.AddTransient<UploadFileCommand>();
                            services.AddTransient<DownloadFileCommand>();
                        });
                })
                .Run();
        }
    }
}
