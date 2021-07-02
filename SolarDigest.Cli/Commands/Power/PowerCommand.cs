using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SolarDigest.Cli.Commands.Download
{
    // Command: power --siteId <siteId> --meterType <meterType> --summaryType <summaryType>
    //                --startDate <startDate> --endDate <endDate> --limit <limit> --startCursor <startCursor>
    // ------------------------------------------------------------------------------------------------------
    //
    // <meterType>      defaults to PRODUCTION
    // <summaryType>    defaults to DAILY_AVERAGE
    // <startDate>      defaults to site start date
    // <endDate>        defaults to the current date (in site local time)
    // <limit>          defaults to null (all data)
    // <startCursor>    defaults to null (first record)
    //
    internal sealed class PowerCommand : ICommand
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PowerCommand> _logger;

        public static string Identifier => "power";

        public PowerCommand(IConfiguration configuration, ILogger<PowerCommand> logger)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task Execute()
        {
            var graphqlUrl = _configuration.GetValue<string>("GraphqlUrl");         // via user secrets / environment variables
            var apiKey = _configuration.GetValue<string>("x-api-key");              // via user secrets / environment variables
            var siteId = _configuration.GetValue<string>("siteId");                 // via command line








            await Task.Delay(1);
        }
    }
}