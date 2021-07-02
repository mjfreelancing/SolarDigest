using AllOverIt.Helpers;
using ConsoleService;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Cli.Commands;
using SolarDigest.Cli.Commands.Download;
using SolarDigest.Cli.Commands.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Cli
{
    internal sealed class SolarDigestCli : IConsoleApp
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IDictionary<string, Type> _commandHandlers = new Dictionary<string, Type>();

        public SolarDigestCli(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider.WhenNotNull(nameof(serviceProvider));

            _commandHandlers.Add(UploadFileCommand.Identifier, typeof(UploadFileCommand));
            _commandHandlers.Add(DownloadFileCommand.Identifier, typeof(DownloadFileCommand));
            _commandHandlers.Add(PowerCommand.Identifier, typeof(PowerCommand));
        }

        public async Task<int> Execute()
        {
            var args = Environment.GetCommandLineArgs();
            
            foreach (var (command, type) in _commandHandlers)
            {
                if (args.Contains(command))
                {
                    try
                    {
                        var handler = _serviceProvider.GetRequiredService(type) as ICommand;
                        await handler!.Execute().ConfigureAwait(false);

                        return 0;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        return -2;
                    }
                }
            }

            return 0;
        }
    }
}