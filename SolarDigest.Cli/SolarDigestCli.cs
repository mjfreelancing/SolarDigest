using AllOverIt.GenericHost;
using AllOverIt.Helpers;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Cli.Commands;
using SolarDigest.Cli.Commands.Download;
using SolarDigest.Cli.Commands.Power;
using SolarDigest.Cli.Commands.Site;
using SolarDigest.Cli.Commands.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Cli
{
    internal sealed class SolarDigestCli : ConsoleAppBase
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IDictionary<string, Type> _commandHandlers = new Dictionary<string, Type>();

        public SolarDigestCli(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider.WhenNotNull(nameof(serviceProvider));

            _commandHandlers.Add(SiteDetailsCommand.Identifier, typeof(SiteDetailsCommand));
            _commandHandlers.Add(SitePowerCommand.Identifier, typeof(SitePowerCommand));
            _commandHandlers.Add(UploadFileCommand.Identifier, typeof(UploadFileCommand));
            _commandHandlers.Add(DownloadFileCommand.Identifier, typeof(DownloadFileCommand));
        }

        public override async Task StartAsync(CancellationToken cancellationToken = default)
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

                        ExitCode = 0;
                        return;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        ExitCode = -2;
                        return;
                    }
                }
            }

            ExitCode = -1;
        }
    }
}