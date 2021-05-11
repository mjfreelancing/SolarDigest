namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestExceptionTable : SolarDigestTableBase, ISolarDigestExceptionTable
    {
        public override string TableName => Constants.Table.Exception;
    }
}