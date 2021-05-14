using SolarDigest.Api.Logging;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestExceptionTable : SolarDigestTableBase, ISolarDigestExceptionTable
    {
        public override string TableName => Constants.Table.Exception;

        public SolarDigestExceptionTable(IFunctionLogger logger)
            : base(logger)
        {
        }
    }
}