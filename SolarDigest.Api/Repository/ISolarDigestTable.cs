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
        Task PutItemAsync<TItem>(TItem item, CancellationToken cancellationToken = default);
    }


    public interface ISolarDigestTable
    {
        string TableName { get; }
    }
}