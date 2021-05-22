using Amazon.DynamoDBv2.DocumentModel;
using SolarDigest.Api.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestTable
    {
        string TableName { get; }

        // todo: consider different overloads
        IAsyncEnumerable<TItem> ScanAsync<TItem>(IEnumerable<string> properties = null, Action<ScanFilter> filterAction = null,
            CancellationToken cancellationToken = default) where TItem : EntityBase;

        Task<TItem> GetItemAsync<TItem>(string id, CancellationToken cancellationToken = default) where TItem : EntityBase;

        // returns all items for a given primary key
        IAsyncEnumerable<TItem> GetItemsAsync<TItem>(string id, CancellationToken cancellationToken = default) where TItem : EntityBase;

        // search for items based on a query definition
        IAsyncEnumerable<TItem> GetItemsAsync<TItem>(QueryOperationConfig config, CancellationToken cancellationToken) where TItem : EntityBase;

        // Add, must not already exist. Assumes has an 'Id' property.
        Task AddItemAsync<TItem>(TItem entity, CancellationToken cancellationToken = default) where TItem : EntityBase;

        // Adds or replaces. Assumes the item has the required properties to satisfy the table's primary/sort keys
        Task PutItemAsync<TItem>(TItem item, CancellationToken cancellationToken = default) where TItem : EntityBase;
        Task PutItemsAsync<TItem>(IEnumerable<TItem> items, CancellationToken cancellationToken = default) where TItem : EntityBase;
    }
}