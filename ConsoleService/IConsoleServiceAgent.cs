using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleService
{
    public interface IConsoleServiceAgent
    {
        Task Run(CancellationToken cancellationToken = default);
        Task Run(Action<ConsoleLifetimeOptions> configureOptions, CancellationToken cancellationToken = default);
    }
}
