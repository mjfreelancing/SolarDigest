using AllOverIt.Helpers;
using ConsoleService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Cli.Commands;
using SolarDigest.Cli.Commands.Download;
using SolarDigest.Cli.Commands.Upload;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Cli
{
    internal sealed class SolarDigestCli : IConsoleApp
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private readonly IDictionary<string, Type> _commandHandlers = new Dictionary<string, Type>();

        public SolarDigestCli(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
            _serviceProvider = serviceProvider.WhenNotNull(nameof(serviceProvider));

            _commandHandlers.Add("upload", typeof(UploadFileCommand));
            _commandHandlers.Add("download", typeof(DownloadFileCommand));
        }

        public async Task<int> Execute()
        {
            foreach (var (command, type) in _commandHandlers)
            {
                if (_configuration.GetValue<string>(command) != null)
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