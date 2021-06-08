using AllOverIt.Helpers;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Models;
using System;

namespace SolarDigest.Api.Data
{
    // HydrateSitePower will raise events that are later used for reporting a summary via email
    public sealed class PowerUpdateHistoryEntity : EntityCompositeBase
    {
        public string Site { get; set; }
        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
        public string Status { get; set; }

        public PowerUpdateHistoryEntity()
        {
        }

        public PowerUpdateHistoryEntity(string siteId, DateTime startDateTime, DateTime endDateTime, PowerUpdateStatus status)
        {
            Site = siteId.WhenNotNull(nameof(siteId));
            StartDateTime = startDateTime.GetSolarDateTimeString();
            EndDateTime = endDateTime.GetSolarDateTimeString();
            Status = $"{status}";

            var startDate = startDateTime.Date.GetSolarDateString();
            Id = $"{siteId}_{startDate}";
            Sort = $"{Guid.NewGuid()}";
        }
    }
}