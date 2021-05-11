using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Services.SolarEdge.Result;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services.SolarEdge
{
    public interface ISolarEdgeApi
    {
        Task<PowerDataResult> GetPowerDetailsAsync(string solarEdgeUri, PowerQuery powerQuery);
        Task<EnergyDataResult> GetEnergyDetailsAsync(string solarEdgeUri, PowerQuery powerQuery);
    }
}