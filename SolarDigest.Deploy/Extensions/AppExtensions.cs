using Amazon.CDK;
using CDKEnvironment = Amazon.CDK.Environment;
using SystemEnvironment = System.Environment;

namespace SolarDigest.Deploy.Extensions
{
    internal static class AppExtensions
    {
        internal static Stack CreateRootStack(this App app, SolarDigestAppProps appProps)
        {
            return new(app, $"{appProps.StackName}V{appProps.Version}", new StackProps
            {
                Description = $"Creates all resources for {appProps.StackName}",
                Env = new CDKEnvironment
                {
                    Account = SystemEnvironment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = SystemEnvironment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
                }
            });
        }
    }
}