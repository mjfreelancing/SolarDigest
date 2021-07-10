using AllOverIt.GenericHost;
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
    // command line args: https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#command-line-configuration-provider
    // key="value"
    // /key "value"
    // --key "value"
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync(options => options.SuppressStatusMessages = true);
        }

        // Not using EF, just doing it this way out of habit
        //
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0
        // If the app uses Entity Framework Core, don't change the name or signature of the CreateHostBuilder method.
        // The Entity Framework Core tools expect to find a CreateHostBuilder method that configures the host without
        // running the app. For more information, see https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return GenericHost
                .CreateConsoleHostBuilder(args)

                .ConfigureHostConfiguration(configHost =>
                {
                    // required for the builder itself, otherwise the DEVELOPMENT environment is not determined
                    configHost.AddEnvironmentVariables();
                })

                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Program>();
                    }
                })

                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddConsole();
                })

                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IConsoleApp, SolarDigestCli>();
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
                    services.AddTransient<SiteDetailsCommand>();
                    services.AddTransient<SitePowerCommand>();
                    services.AddTransient<UploadFileCommand>();
                    services.AddTransient<DownloadFileCommand>();
                });
        }
    }
}
