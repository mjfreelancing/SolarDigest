using SolarDigest.Api.Logging;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerMonthlyTable : SolarDigestTableBase, ISolarDigestPowerMonthlyTable
    {
        public override string TableName => Constants.Table.PowerMonthly;

        public SolarDigestPowerMonthlyTable(IFunctionLogger logger)
            : base(logger)
        {
        }
    }
}