using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using SolarDigest.Api.Extensions;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class SolarDigestDynamoDb : ISolarDigestDynamoDb
    {
        private readonly Lazy<AmazonDynamoDBClient> _dbClient = new Lazy<AmazonDynamoDBClient>(() => new AmazonDynamoDBClient());
        private AmazonDynamoDBClient DbClient => _dbClient.Value;

        public async Task<TItem> GetItemAsync<TItem>(string tableName, string id)
        {
            var table = Table.LoadTable(DbClient, new TableConfig(tableName));
            var document = await table.GetItemAsync(new Primitive(id));

            return JsonConvert.DeserializeObject<TItem>(document.ToJson());

            //var key = new Dictionary<string, AttributeValue>
            //{
            //    {"Id", new AttributeValue {S = id}}
            //};

            //var response = await dbClient.GetItemAsync(tableName, key);

            //var item = response.Item;

            //var itemJson = Document.FromAttributeMap(item).ToJson();

            //return JsonConvert.DeserializeObject<TItem>(itemJson);
        }

        public Task PutItemAsync<TItem>(string tableName, TItem item)
        {
            return DbClient.PutItemAsync(tableName, item);
        }
    }
}