using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    public interface ISolarDigestDynamoDb
    {
        Task<TItem> GetItemAsync<TItem>(string tableName, string id);
        Task PutItemAsync<TItem>(string tableName, TItem item);
    }
}