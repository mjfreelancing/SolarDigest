namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestSiteTable : SolarDigestTableBase, ISolarDigestSiteTable
    {
        public override string TableName => Constants.Table.Site;
    }
}