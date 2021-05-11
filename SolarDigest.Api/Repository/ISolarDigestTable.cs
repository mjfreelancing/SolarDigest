using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestTable
    {
        string TableName { get; }
        Task<TItem> GetItemAsync<TItem>(string id, CancellationToken cancellationToken = default);
        Task PutItemAsync<TItem>(TItem item, CancellationToken cancellationToken = default);
    }

    public interface ISolarDigestSiteTable : ISolarDigestTable
    {

    }

    public interface ISolarDigestExceptionTable : ISolarDigestTable
    {

    }
}