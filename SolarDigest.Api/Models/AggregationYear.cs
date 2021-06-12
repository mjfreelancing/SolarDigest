using System;

namespace SolarDigest.Api.Models
{
    public sealed class AggregationYear : AggregationPeriodBase
    {
        public AggregationYear(DateTime startDate, DateTime endDate, int year)
            : base(startDate, endDate, year)
        {
        }
    }
}