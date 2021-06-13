using AutoMapper;
using SolarDigest.Api.Logging;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestEnergyCostsTable : SolarDigestTableBase, ISolarDigestEnergyCostsTable
    {
        public override string TableName => Constants.Table.EnergyCosts;

        public SolarDigestEnergyCostsTable(IMapper mapper, ISolarDigestLogger logger)
            : base(mapper, logger)
        {
        }
    }
}