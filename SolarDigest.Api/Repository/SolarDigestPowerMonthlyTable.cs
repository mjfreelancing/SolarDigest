namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerMonthlyTable : SolarDigestTableBase, ISolarDigestPowerMonthlyTable
    {
        public override string TableName => Constants.Table.PowerMonthly;
    }
}