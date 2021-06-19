using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace ConsoleService
{
    public sealed class ConsoleHost
    {
        public static IConsoleServiceAgent Build(string[] args, Action<IHostBuilder> hostBuilder)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddEnvironmentVariables();
                    configHost.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<HostedConsoleService>();
                });

            hostBuilder.Invoke(builder);

            return new ConsoleServiceAgent(builder);
        }
    }
}