using Amazon.DynamoDBv2.DocumentModel;

namespace SolarDigest.Api.Services
{
    public interface ISolarDigestDynamoDb
    {
        Table GetTable(string tableName);
    }
}