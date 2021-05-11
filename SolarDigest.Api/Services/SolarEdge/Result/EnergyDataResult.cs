using AllOverIt.Helpers;
using SolarDigest.Api.Services.SolarEdge.Response;
using System.Net;

namespace SolarDigest.Api.Services.SolarEdge.Result
{
    public class EnergyDataResult
    {
        public HttpStatusCode StatusCode { get; }
        public EnergyDataDto EnergyData { get; }
        public bool IsError => StatusCode != HttpStatusCode.OK;

        public EnergyDataResult(EnergyDataDto energyData)
        {
            StatusCode = HttpStatusCode.OK;
            EnergyData = energyData.WhenNotNull(nameof(energyData));
        }

        public static EnergyDataResult Error(HttpStatusCode statusCode)
        {
            return new EnergyDataResult(statusCode);
        }

        private EnergyDataResult(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}