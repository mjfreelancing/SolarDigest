namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestSiteTable : ISolarDigestTable, IReadDynamoDbTable, IWriteDynamoDbTable
    {
    }
}