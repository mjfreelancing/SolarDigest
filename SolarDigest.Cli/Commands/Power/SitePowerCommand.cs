using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SolarDigest.Graphql;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Cli.Commands.Power
{
    // Command: power --siteId <siteId> --meterType <meterType> --summaryType <summaryType>
    //                --startDate <startDate> --endDate <endDate> --limit <limit> --startCursor <startCursor>
    // ------------------------------------------------------------------------------------------------------
    //
    // <meterType>      defaults to Production
    // <summaryType>    defaults to DailyAverage
    // <startDate>      defaults to site start date
    // <endDate>        defaults to the current date (in site local time)
    // <limit>          defaults to null (all data)
    // <startCursor>    defaults to null (first record)
    //
    internal sealed class SitePowerCommand : ICommand
    {
        private readonly ISolarDigestGraphql _solarDigestGraphql;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SitePowerCommand> _logger;

        public static string Identifier => "power";

        public SitePowerCommand(ISolarDigestGraphql solarDigestGraphql, IConfiguration configuration, ILogger<SitePowerCommand> logger)
        {
            _solarDigestGraphql = solarDigestGraphql.WhenNotNull(nameof(solarDigestGraphql));
            _configuration = configuration.WhenNotNull(nameof(configuration));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task Execute()
        {
            // all via command line
            var siteId = _configuration.GetValue<string>("siteId");
            var meterType = _configuration.GetValue<string>("meterType");
            var summaryType = _configuration.GetValue<string>("summaryType");
            var startDate = _configuration.GetValue<string>("startDate");
            var endDate = _configuration.GetValue<string>("endDate");
            var limit = _configuration.GetValue<int?>("limit");
            var startCursor = _configuration.GetValue<string>("startCursor");

            for (; ; )
            {
                Console.WriteLine();
                Console.WriteLine("Querying...");
                Console.WriteLine();

                var sitePower = await _solarDigestGraphql.GetSitePowerAsync(siteId, meterType, summaryType, startDate, endDate, limit, startCursor).ConfigureAwait(false);

                if (sitePower.TotalCount == 0)
                {
                    Console.WriteLine("No power data for the parameters provided.");
                    return;
                }

                foreach (var edge in sitePower.Edges)
                {
                    var node = edge.Node;

                    Console.WriteLine($"[{node.Time}] ({edge.Cursor}) = {node.Watts} Watts / {node.WattHour} Watt Hour");
                }

                var prevPageCursor = sitePower.PageInfo?.PreviousPageCursor;
                var nextPageCursor = sitePower.PageInfo?.NextPageCursor;

                var hasPreviousPage = !prevPageCursor?.IsNullOrEmpty() ?? false;
                var hasNextPage = !nextPageCursor?.IsNullOrEmpty() ?? false;

                if (!hasPreviousPage && !hasNextPage)
                {
                    Console.WriteLine("No more power data for the parameters provided.");
                    return;
                }

                Console.WriteLine();

                if (hasPreviousPage)
                {
                    Console.WriteLine("Press P to show the previous page");
                }

                if (hasNextPage)
                {
                    Console.WriteLine("Press N to show the next page");
                }

                Console.WriteLine("Press Q to quit page navigation");
                Console.WriteLine();

                for (; ; )
                {
                    var key = Console.ReadKey(true).Key;

                    if (hasPreviousPage && key == ConsoleKey.P)
                    {
                        startCursor = sitePower.PageInfo.PreviousPageCursor;
                        break;
                    }

                    if (hasNextPage && key == ConsoleKey.N)
                    {
                        startCursor = sitePower.PageInfo.NextPageCursor;
                        break;
                    }

                    if (key == ConsoleKey.Q)
                    {
                        return;
                    }
                }
            }
        }
    }
}
