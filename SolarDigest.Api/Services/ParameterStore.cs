using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class ParameterStore : IParameterStore
    {
        public async Task<string> GetByNameAsync(string name)
        {
            using (var client = new AmazonSimpleSystemsManagementClient(Constants.RegionEndpoint))
            {
                var request = new GetParameterRequest
                {
                    Name = name
                };

                // Currently assuming response.HttpStatusCode == HttpStatusCode.OK
                var response = await client.GetParameterAsync(request);

                return response.Parameter.Value;
            }
        }

        public async Task<IDictionary<string, string>> GetByPathAsync(string path)
        {
            using (var client = new AmazonSimpleSystemsManagementClient(Constants.RegionEndpoint))
            {
                var request = new GetParametersByPathRequest
                {
                    Path = path
                };

                // Currently assuming response.HttpStatusCode == HttpStatusCode.OK
                var response = await client.GetParametersByPathAsync(request);

                return response.Parameters
                    .Select(item => new KeyValuePair<string, string>(item.Name, item.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }
    }
}