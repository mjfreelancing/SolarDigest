using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Services.SolarEdge.Response;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services.SolarEdge
{
    public interface ISolarEdgeApi
    {
        Task<PowerDataDto> GetPowerDetailsAsync(string apiKey, PowerQuery powerQuery);
        Task<EnergyDataDto> GetEnergyDetailsAsync(string apiKey, PowerQuery powerQuery);
    }
}