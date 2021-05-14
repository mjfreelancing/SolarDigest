using SolarDigest.Api.Logging;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerTable : SolarDigestTableBase, ISolarDigestPowerTable
    {
        public override string TableName => Constants.Table.Power;

        public SolarDigestPowerTable(IFunctionLogger logger)
            : base(logger)
        {
        }
    }
}