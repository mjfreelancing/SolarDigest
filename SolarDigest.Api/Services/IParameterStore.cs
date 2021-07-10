using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    public interface IParameterStore
    {
        Task<string> GetByNameAsync(string name);
        Task<IDictionary<string, string>> GetByPathAsync(string path);
    }
}