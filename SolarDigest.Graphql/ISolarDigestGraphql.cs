using SolarDigest.Graphql.Responses;
using System.Threading.Tasks;

namespace SolarDigest.Graphql
{
    public interface ISolarDigestGraphql
    {
        Task<GetSiteResponse> GetSiteAsync(string siteId);

        Task<GetSitePowerResponse> GetSitePowerAsync(string siteId, string meterType, string summaryType, string startDate, string endDate, int? limit, string startCursor);
    }
}
