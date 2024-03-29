﻿using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using AutoMapper;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using SolarDigest.Api.Data;
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
        // would normally go the max limit of 25 requests but keeping it low to help with throttling on the free plan
        private const int PutBatchSize = 4;

        // A factory method to ensure each use is distinct
        private static AsyncRetryPolicy GetProvisionedThroughputExceededRetryPolicy() => Policy
            .Handle<ProvisionedThroughputExceededException>()
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        private static readonly AmazonDynamoDBClient DbClient
            = new(new AmazonDynamoDBConfig
            {
                RetryMode = RequestRetryMode.Standard,
                MaxErrorRetry = 10,
                
            });

        protected ISolarDigestTable TableImpl => this;
        protected IMapper Mapper { get; }
        protected ISolarDigestLogger Logger { get; }

        public abstract string TableName { get; }

        protected SolarDigestTableBase(IMapper mapper, ISolarDigestLogger logger)
        {
            Mapper = mapper.WhenNotNull(nameof(mapper));
            Logger = logger.WhenNotNull(nameof(logger));
        }

        async IAsyncEnumerable<TItem> ISolarDigestTable.ScanAsync<TItem>(IEnumerable<string> properties, Action<ScanFilter> filterAction,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var filter = new ScanFilter();

            filterAction?.Invoke(filter);       // eg., filter.AddCondition( "year", ScanOperator.Between, new DynamoDBEntry[ ] { 1950, 1959 } );

            var scanConfig = new ScanOperationConfig
            {
                Filter = filter,
                // Select defaults to SelectValues.AllAttributes
            };


            if (properties != null)
            {
                scanConfig.Select = SelectValues.SpecificAttributes;
                scanConfig.AttributesToGet = properties.ToList();
            }

            var table = Table.LoadTable(DbClient, new TableConfig(TableName));
            var search = table.Scan(scanConfig);

            var results = GetSearchResultsAsync<TItem>(search, cancellationToken);

            await foreach (var result in results.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                yield return result;
            }
        }

        async Task<TItem> ISolarDigestTable.GetItemAsync<TItem>(string id, CancellationToken cancellationToken)
        {
            var table = Table.LoadTable(DbClient, new TableConfig(TableName));
            var document = await table.GetItemAsync(id, cancellationToken).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TItem>(document.ToJson());
        }

        async IAsyncEnumerable<TItem> ISolarDigestTable.GetItemsAsync<TItem>(string id, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var table = Table.LoadTable(DbClient, new TableConfig(TableName));

            var search = table.Query(id, new Expression());

            var results = GetSearchResultsAsync<TItem>(search, cancellationToken);

            await foreach (var result in results.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                yield return result;
            }
        }

        async IAsyncEnumerable<TItem> ISolarDigestTable.GetItemsAsync<TItem>(QueryOperationConfig config, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var table = Table.LoadTable(DbClient, new TableConfig(TableName));

            var search = table.Query(config);

            var results = GetSearchResultsAsync<TItem>(search, cancellationToken);

            await foreach (var result in results.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                yield return result;
            }
        }

        async Task ISolarDigestTable.AddItemAsync<TItem>(TItem entity, CancellationToken cancellationToken)
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

        Task ISolarDigestTable.PutItemAsync<TItem>(TItem entity, CancellationToken cancellationToken)
        {
            var attributeValues = GetAttributeValues(entity);

            var putItem = new PutItemRequest
            {
                TableName = TableName,
                Item = attributeValues
            };

            return DbClient.PutItemAsync(putItem, cancellationToken);
        }

        async Task ISolarDigestTable.PutBatchItemsAsync<TItem>(IEnumerable<TItem> items, CancellationToken cancellationToken)
        {
            var entities = items.AsReadOnlyCollection();

            var responses = GetPutBatchResponses(entities, cancellationToken);
            
            await foreach (var response in responses.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                var unprocessed = response.UnprocessedItems;

                if (unprocessed.Count > 0)
                {
                    Logger.LogDebug($"Unprocessed count = {unprocessed.Count}");
                }
            }
        }

        private async IAsyncEnumerable<BatchWriteItemResponse> GetPutBatchResponses<TItem>(IEnumerable<TItem> items,
            [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            // A single call to BatchWriteItemAsync can write up to 16 MB of data, which can comprise
            // as many as 25 put requests. Individual items to be written can be as large as 400 KB.
            var entities = items.AsReadOnlyCollection();

            if (entities.Any())
            {
                var batches = entities.Batch(PutBatchSize).AsReadOnlyCollection();

                Logger.LogDebug($"Processing {entities.Count} entities across {batches.Count} batches of PUT requests");

                foreach (var batch in batches)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    var requests = batch
                        .Select(entity => new PutRequest { Item = GetAttributeValues(entity) })
                        .Select(putRequest => new WriteRequest(putRequest))
                        .ToList();

                    var batchRequest = new BatchWriteItemRequest(new Dictionary<string, List<WriteRequest>> { { TableName, requests } });

                    var response = await GetProvisionedThroughputExceededRetryPolicy()
                        .ExecuteAsync(() => DbClient.BatchWriteItemAsync(batchRequest, cancellationToken))
                        .ConfigureAwait(false);

                    yield return response;
                }
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

        private static async IAsyncEnumerable<TItem> GetSearchResultsAsync<TItem>(Search search, [EnumeratorCancellation] CancellationToken cancellationToken)
            where TItem : EntityBase
        {
            while (!search.IsDone)
            {
                var documents = await search.GetNextSetAsync(cancellationToken).ConfigureAwait(false);

                foreach (var document in documents)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    yield return JsonConvert.DeserializeObject<TItem>(document.ToJson());
                }
            }
        }
    }
}