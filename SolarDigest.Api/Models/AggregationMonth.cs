using System;

namespace SolarDigest.Api.Models
{
    public sealed class AggregationMonth : AggregationPeriodBase
    {
        public int Month { get; }

        public AggregationMonth(DateTime startDate, DateTime endDate, int year, int month)
            : base(startDate, endDate, year)
        {
            Month = month;
        }
    }
}