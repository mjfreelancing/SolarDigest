using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Services.SolarEdge.Response;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services.SolarEdge
{
    public interface ISolarEdgeApi
    {
        Task<PowerDataDto> GetPowerDetailsAsync(string solarEdgeUri, PowerQuery powerQuery);
        Task<EnergyDataDto> GetEnergyDetailsAsync(string solarEdgeUri, PowerQuery powerQuery);
    }
}