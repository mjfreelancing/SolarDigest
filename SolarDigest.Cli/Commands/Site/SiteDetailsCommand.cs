using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SolarDigest.Graphql;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Cli.Commands.Site
{
    internal sealed class SiteDetailsCommand : ICommand
    {
        private readonly ISolarDigestGraphql _solarDigestGraphql;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SiteDetailsCommand> _logger;

        public static string Identifier => "site";

        public SiteDetailsCommand(ISolarDigestGraphql solarDigestGraphql, IConfiguration configuration, ILogger<SiteDetailsCommand> logger)
        {
            _solarDigestGraphql = solarDigestGraphql.WhenNotNull(nameof(solarDigestGraphql));
            _configuration = configuration.WhenNotNull(nameof(configuration));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task Execute()
        {
            // via command line
            var siteId = _configuration.GetValue<string>("siteId");

            var site = await _solarDigestGraphql.GetSiteAsync(siteId).ConfigureAwait(false);

            Console.WriteLine();

            foreach (var (key, value) in site.ToPropertyDictionary())
            {
                Console.WriteLine($"{key} = {value}");
            }

            Console.WriteLine();
        }
    }
}