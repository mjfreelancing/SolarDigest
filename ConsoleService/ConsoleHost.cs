using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ConsoleService
{
    public sealed class ConsoleHost
    {
        public static IConsoleServiceAgent Build(string[] args, Action<IHostBuilder> hostBuilder)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<HostedConsoleService>();
                });

            hostBuilder.Invoke(builder);

            return new ConsoleServiceAgent(builder);
        }
    }
}