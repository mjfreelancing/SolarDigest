using AllOverIt.Extensions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal abstract class SolarDigestTableBase : ISolarDigestTable
    {
        private readonly Lazy<AmazonDynamoDBClient> _dbClient = new(() => new AmazonDynamoDBClient());
        private AmazonDynamoDBClient DbClient => _dbClient.Value;

        public abstract string TableName { get; }

        public async Task<TItem> GetItemAsync<TItem>(string id, CancellationToken cancellationToken)
        {
            var table = Table.LoadTable(DbClient, new TableConfig(TableName));
            var document = await table.GetItemAsync(new Primitive(id), cancellationToken);

            return JsonConvert.DeserializeObject<TItem>(document.ToJson());
        }

        public Task PutItemAsync<TItem>(TItem entity, CancellationToken cancellationToken)
        {
            var siteValues = entity.ToPropertyDictionary();

            var attributeValues = siteValues.ToDictionary(
                kvp => kvp.Key,
                kvp => CreateAttributeValue(kvp.Value));

            var putItem = new PutItemRequest
            {
                TableName = TableName,
                Item = attributeValues
            };

            return DbClient.PutItemAsync(putItem, cancellationToken);
        }

        private static AttributeValue CreateAttributeValue(object value)
        {
            var valueType = value.GetType();

            if (valueType == typeof(string))
            {
                return new AttributeValue { S = $"{value}" };
            }

            throw new InvalidOperationException($"Unexpected attribute value type: {valueType.FullName}");
        }
    }
}