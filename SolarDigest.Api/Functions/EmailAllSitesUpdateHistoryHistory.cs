using AllOverIt.Extensions;
using HtmlBuilders;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Models;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class EmailAllSitesUpdateHistory : FunctionBase<EmailAllSitesUpdateHistoryPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<EmailAllSitesUpdateHistoryPayload> context)
        {
            var logger = context.Logger;
            logger.LogDebug("Email update history for all sites");

            var currentTimeUtc = DateTime.UtcNow;

            var serviceProvider = context.ScopedServiceProvider;
            var siteTable = serviceProvider.GetRequiredService<ISolarDigestSiteTable>();

            // only retrieve the properties we need
            var sites = siteTable!.GetAllSitesAsync(new[]
            {
                nameof(Site.Id), nameof(Site.TimeZoneId), nameof(Site.StartDate), nameof(Site.LastSummaryDate)
            });

            await foreach (var site in sites)
            {
                var siteLocalTime = site.UtcToLocalTime(currentTimeUtc);

                logger.LogDebug($"Converted {currentTimeUtc.GetSolarDateTimeString()} to site {site.Id} local time " +
                                $"{siteLocalTime.GetSolarDateTimeString()} ({site.TimeZoneId})");

                // check subsequent hours in case a trigger was missed
                if (siteLocalTime.Hour >= Constants.RefreshHour.UpdateHistoryEmail)
                {
                    var lastSummaryDate = site.GetLastSummaryDate();
                    var nextEndDate = siteLocalTime.Date.AddDays(-1);         // not reporting the current day as it is not yet over

                    if (nextEndDate > lastSummaryDate)
                    {
                        var updateHistoryTable = serviceProvider.GetRequiredService<ISolarDigestPowerUpdateHistoryTable>();

                        var historyItems = await updateHistoryTable.GetPowerUpdatesAsyncEnumerable(site.Id, lastSummaryDate, nextEndDate)
                            .ToListAsync()
                            .ConfigureAwait(false);

                        logger.LogDebug($"Generating email content for {historyItems.Count} items");

                        var emailContent = BuildHtml(site, historyItems);

                        // todo: send the email and update the last summary date
                        logger.LogDebug($"Content: {emailContent}");


                    }
                }
            }

            logger.LogDebug("All Sites have been iterated");

            return true;
        }

        private static string BuildHtml(Site site, IEnumerable<PowerUpdateHistory> historyItems)
        {
            //var history = historyItems.Where(item => item.Status != $"{PowerUpdateStatus.Started}");

            var items = historyItems.OrderBy(item => item.TimestampUtc).AsReadOnlyList();

            var table = CreateTable(site, items);

            var div = HtmlTags.Div
                .Style("margin-bottom", "24")
                .Append(table);

            var body = HtmlTags.Body
                .Style("width", "640")
                .Style("margin-left", "auto")
                .Style("margin-right", "auto")
                .Append(div);

            return HtmlTags.Html
                .Append(body)
                .ToHtmlString();
        }

        private static HtmlTag CreateTable(Site site, IEnumerable<PowerUpdateHistory> items)
        {
            var table = HtmlTags.Table
                .Style("width", "640")
                .Style("border-collapse", "collapse")
                .Attribute("cellpadding", "4")
                .Attribute("cellspacing", "4")
                .Attribute("border", "1");

            var tableHeader = HtmlTags.Tr.Append(
                HtmlTags.Th.Attribute("colspan", "4")
                    .Style("background-color", "#404040")
                    .Style("color", "white")
                    .Style("padding", "8")
                    .Style("margin-bottom", "4")
                    .Append($"Update History for Site: {site.Id}")
            );

            table = table.Append(tableHeader);

            table = table.Append(CreateColumnHeaders());

            foreach (var item in items)
            {
                var row = HtmlTags.Tr.Style("text-align", "center");

                if (item.Status == $"{PowerUpdateStatus.Error}")
                {
                    row = row.Style("background-color", "red").Style("color", "white");
                }

                var localTimestamp = site.UtcToLocalTime(item.TimestampUtc.ParseSolarDateTime());

                row = row.Append(
                    HtmlTags.Td.Append(localTimestamp.GetSolarDateTimeString()),
                    HtmlTags.Td.Append(item.StartDateTime),
                    HtmlTags.Td.Append(item.EndDateTime),
                    HtmlTags.Td.Append(item.Status)
                );

                table = table.Append(row);
            }

            return table;
        }

        private static HtmlTag CreateColumnHeaders()
        {
            return HtmlTags.Tr
                .Style("text-align", "center")
                .Style("background-color", "#457b9d").Style("color", "white")
                .Append(
                    HtmlTags.Td.Append(HtmlTags.Strong.Append("Triggered")),
                    HtmlTags.Td.Append(HtmlTags.Strong.Append("Start Time")),
                    HtmlTags.Td.Append(HtmlTags.Strong.Append("End Time")),
                    HtmlTags.Td.Append(HtmlTags.Strong.Append("Status"))
                );
        }
    }

        public static class AsyncEnumerableExtensions
        {
            public static async Task<List<TType>> ToListAsync<TType>(this IAsyncEnumerable<TType> items,
                CancellationToken cancellationToken = default)
            {
                var listItems = new List<TType>();
                
                await foreach (var item in items.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    listItems.Add(item);
                }

                return listItems;
            }
        }
}