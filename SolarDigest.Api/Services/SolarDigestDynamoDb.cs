using AllOverIt.Helpers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace SolarDigest.Api.Services
{
    internal sealed class SolarDigestDynamoDb : ISolarDigestDynamoDb
    {
        private readonly IConfiguration _configuration;

        public SolarDigestDynamoDb(IConfiguration configuration)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
        }

        public Table GetTable(string tableName)
        {
            var accessKey = _configuration["AwsAccessKey"];
            var secretKey = _configuration["AwsSecretKey"];

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var dbClient = new AmazonDynamoDBClient(credentials);

            return Table.LoadTable(dbClient, tableName);
        }
    }
}