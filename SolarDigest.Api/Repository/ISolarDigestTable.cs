using Amazon.DynamoDBv2.DocumentModel;
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
        IAsyncEnumerable<TItem> ScanAsync<TItem>(Action<ScanFilter> filterAction = null, IEnumerable<string> properties = null, CancellationToken cancellationToken = default);

        Task<TItem> GetItemAsync<TItem>(string id, CancellationToken cancellationToken = default);

        // returns all items for a given primary key
        IAsyncEnumerable<TItem> GetItemsAsync<TItem>(string id, CancellationToken cancellationToken = default);

        // Add, must not already exist. Assumes has an 'Id' property.
        Task AddItemAsync<TItem>(TItem entity, CancellationToken cancellationToken = default);

        // Adds or replaces. Assumes the item has the required properties to satisfy the table's primary/sort keys
        Task PutItemAsync<TItem>(TItem item, CancellationToken cancellationToken = default);
        Task PutItemsAsync<TItem>(IEnumerable<TItem> items, CancellationToken cancellationToken = default);
    }
}