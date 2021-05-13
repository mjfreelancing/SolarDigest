using AllOverIt.Extensions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using SolarDigest.Api.Exceptions;
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
            var document = await table.GetItemAsync(id, cancellationToken);

            return JsonConvert.DeserializeObject<TItem>(document.ToJson());
        }

        public async Task AddItemAsync<TItem>(TItem entity, CancellationToken cancellationToken)
        {
            var values = entity.ToPropertyDictionary();

            var attributeValues = values.ToDictionary(
                kvp => kvp.Key,
                kvp => CreateAttributeValue(kvp.Value));

            // make sure the record does not already exist
            var table = Table.LoadTable(DbClient, new TableConfig(TableName));
            var existingItem = await table.GetItemAsync(attributeValues["Id"].S, cancellationToken);

            if (existingItem == null)
            {
                var putItem = new PutItemRequest
                {
                    TableName = TableName,
                    Item = attributeValues
                };

                await DbClient.PutItemAsync(putItem, cancellationToken);
            }
            else
            {
                throw new DynamoDbConflictException($"The record with key '{existingItem["Id"]}' in table '{TableName}' already exists");
            }
        }

        public Task PutItemAsync<TItem>(TItem entity, CancellationToken cancellationToken)
        {
            var values = entity.ToPropertyDictionary();

            var attributeValues = values.ToDictionary(
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