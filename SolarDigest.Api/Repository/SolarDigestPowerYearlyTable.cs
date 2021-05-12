namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerYearlyTable : SolarDigestTableBase, ISolarDigestPowerYearlyTable
    {
        public override string TableName => Constants.Table.PowerYearly;
    }
}