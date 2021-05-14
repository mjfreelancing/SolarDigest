using SolarDigest.Api.Logging;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerUpdateHistoryTable : SolarDigestTableBase, ISolarDigestPowerUpdateHistoryTable
    {
        public override string TableName => Constants.Table.PowerUpdateHistory;

        public SolarDigestPowerUpdateHistoryTable(IFunctionLogger logger)
            : base(logger)
        {
        }
    }
}