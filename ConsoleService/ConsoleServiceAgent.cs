using AllOverIt.Helpers;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleService
{
    internal sealed class ConsoleServiceAgent : IConsoleServiceAgent
    {
        private readonly IHostBuilder _hostBuilder;

        public ConsoleServiceAgent(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder.WhenNotNull(nameof(hostBuilder));
        }

        public Task Run(CancellationToken cancellationToken)
        {
            return _hostBuilder.RunConsoleAsync(cancellationToken);
        }

        public Task Run(Action<ConsoleLifetimeOptions> configureOptions, CancellationToken cancellationToken)
        {
            return _hostBuilder.RunConsoleAsync(configureOptions, cancellationToken);
        }
    }
}