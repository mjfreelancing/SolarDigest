using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Polly;
using SolarDigest.Api.Exceptions;
using SolarDigest.Api.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal abstract class SolarDigestTableBase : ISolarDigestTable
    {
        private readonly IFunctionLogger _logger;
        private readonly Lazy<AmazonDynamoDBClient> _dbClient = new(() => new AmazonDynamoDBClient());
        private AmazonDynamoDBClient DbClient => _dbClient.Value;

        public abstract string TableName { get; }

        public SolarDigestTableBase(IFunctionLogger logger)
        {
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task<TItem> GetItemAsync<TItem>(string id, CancellationToken cancellationToken)
        {
            var table = Table.LoadTable(DbClient, new TableConfig(TableName));
            var document = await table.GetItemAsync(id, cancellationToken).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TItem>(document.ToJson());
        }

        public async Task AddItemAsync<TItem>(TItem entity, CancellationToken cancellationToken)
        {
            var attributeValues = GetAttributeValues(entity);

            // make sure the record does not already exist
            var table = Table.LoadTable(DbClient, new TableConfig(TableName));
            var existingItem = await table.GetItemAsync(attributeValues["Id"].S, cancellationToken).ConfigureAwait(false);

            if (existingItem == null)
            {
                var putItem = new PutItemRequest
                {
                    TableName = TableName,
                    Item = attributeValues
                };

                await DbClient.PutItemAsync(putItem, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new DynamoDbConflictException($"The record with key '{existingItem["Id"]}' in table '{TableName}' already exists");
            }
        }

        public Task PutItemAsync<TItem>(TItem entity, CancellationToken cancellationToken)
        {
            var attributeValues = GetAttributeValues(entity);

            var putItem = new PutItemRequest
            {
                TableName = TableName,
                Item = attributeValues
            };

            return DbClient.PutItemAsync(putItem, cancellationToken);
        }

        public async Task PutItemsAsync<TItem>(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            // todo: add polly for failure handling

            // todo: handle ProvisionedThroughputExceededException
            // todo: need to check for failed items and re-try

            var entities = items.AsReadOnlyCollection();

            await foreach (var response in GetPutBatchResponses(entities, cancellationToken))
            {
                var unprocessed = response.UnprocessedItems;

                if (unprocessed.Count > 0)
                {
                    _logger.LogDebug($"Unprocessed count = {unprocessed.Count}");
                }
            }
        }

        private async IAsyncEnumerable<BatchWriteItemResponse> GetPutBatchResponses<TItem>(IEnumerable<TItem> items,
            [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            // A single call to BatchWriteItemAsync can write up to 16 MB of data, which can comprise
            // as many as 25 put requests. Individual items to be written can be as large as 400 KB.
            var entities = items.AsReadOnlyCollection();
            var batches = entities.Batch(25).AsReadOnlyCollection();

            _logger.LogDebug($"Processing {entities.Count} entities across {batches.Count} batches of PUT requests");

            var retryPolicy = Polly.Policy
                .Handle<ProvisionedThroughputExceededException>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            foreach (var batch in batches)
            {
                var requests = batch
                    .Select(entity => new PutRequest {Item = GetAttributeValues(entity)})
                    .Select(putRequest => new WriteRequest(putRequest))
                    .ToList();

                var batchRequest = new BatchWriteItemRequest(new Dictionary<string, List<WriteRequest>> { { TableName, requests } });

                var response = await retryPolicy.ExecuteAsync(() => DbClient.BatchWriteItemAsync(batchRequest, cancellationToken));
                
                yield return response;
            }
        }

        private static Dictionary<string, AttributeValue> GetAttributeValues<TItem>(TItem entity)
        {
            var values = entity.ToPropertyDictionary();

            return values.ToDictionary(
                kvp => kvp.Key,
                kvp => CreateAttributeValue(kvp.Value));
        }

        private static AttributeValue CreateAttributeValue(object value)
        {
            var valueType = value.GetType();

            if (valueType == typeof(string))
            {
                return new AttributeValue { S = $"{value}" };
            }
            
            if (valueType.IsFloatingType() || valueType.IsIntegralType())
            {
                return new AttributeValue {N = $"{value}"};
            }

            throw new InvalidOperationException($"Unexpected attribute value type: {valueType.FullName}");
        }
    }
}