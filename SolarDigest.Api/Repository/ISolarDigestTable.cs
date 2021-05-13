using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface IReadDynamoDbTable
    {
        Task<TItem> GetItemAsync<TItem>(string id, CancellationToken cancellationToken = default);
    }

    public interface IWriteDynamoDbTable
    {
        // Add, must not already exist. Assumes has an 'Id' property.
        Task AddItemAsync<TItem>(TItem entity, CancellationToken cancellationToken = default);

        // Adds or replaces. Assumes the item has the required properties to satisfy the table's primary/sort keys
        Task PutItemAsync<TItem>(TItem item, CancellationToken cancellationToken = default);
    }


    public interface ISolarDigestTable
    {
        string TableName { get; }
    }
}