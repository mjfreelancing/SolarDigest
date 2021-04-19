using AllOverIt.Extensions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Extensions
{
    public static class DynamoDbExtensions
    {
        public static Task<PutItemResponse> PutItemAsync<TEntity>(this AmazonDynamoDBClient dbClient, string tableName, TEntity entity, CancellationToken cancellationToken = default)
        {
            var siteValues = entity.ToPropertyDictionary();

            var attributeValues = siteValues.ToDictionary(
                kvp => kvp.Key,
                kvp => CreateAttributeValue(kvp.Value));

            var putItem = new PutItemRequest
            {
                TableName = tableName,
                Item = attributeValues
            };

            return dbClient.PutItemAsync(putItem, cancellationToken);
        }

        private static AttributeValue CreateAttributeValue(object value)
        {
            var valueType = value.GetType();

            if (valueType == typeof(string))
            {
                return new AttributeValue {S = $"{value}"};
            }

            throw new InvalidOperationException($"Unexpected attribute value type: {valueType.FullName}");
        }
    }
}