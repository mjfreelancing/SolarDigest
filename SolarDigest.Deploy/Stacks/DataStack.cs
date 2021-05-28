using Amazon.CDK;
using SolarDigest.Deploy.Constructs;
using SolarDigest.Deploy.Extensions;

namespace SolarDigest.Deploy.Stacks
{
    internal sealed class DataStack
    {
        public static App CreateApp()
        {
            var app = new App();

            var apiProps = new SolarDigestAppProps
            {
                AppName = $"{Constants.AppName}Data",
                Version = Constants.DataVersion,
            };

            var stack = app.CreateRootStack(apiProps);

            _ = new DynamoDbTables(stack, apiProps);

            return app;
        }
    }
}