using Amazon.CDK;

namespace SolarDigest.Deploy.Extensions
{
    internal static class AppExtensions
    {
        internal static Stack CreateRootStack(this App app, SolarDigestAppProps appProps)
        {
            return new(app, $"{appProps.StackName}V{appProps.Version}", new StackProps
            {
                Description = $"Creates all resources for {appProps.StackName}",
                Env = new Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
                }
            });
        }
    }
}