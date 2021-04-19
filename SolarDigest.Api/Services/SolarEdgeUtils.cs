using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal static class SolarEdgeUtils
    {
        // NOTE: This is eventually to be used when actually calling the SolarEdge API - it doesn't belong here.
        public static async Task<string> GetApiKey()            // create a service that takes a paramName as input
        {
            using (var client = new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.APSoutheast2))
            {
                var request = new GetParameterRequest
                {
                    Name = "/SolarDigest/SolarEdgeApiKey"
                };

                var response = await client.GetParameterAsync(request);

                return response.Parameter.Value;
            }
        }
    }
}