using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Services.SolarEdge.Response;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services.SolarEdge
{
    public interface ISolarEdgeApi
    {
        Task<PowerDataDto> GetPowerDetailsAsync(PowerQuery powerQuery);
        Task<EnergyDataDto> GetEnergyDetailsAsync(PowerQuery powerQuery);
    }
}