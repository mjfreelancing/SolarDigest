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

            var stack = app.CreateRootStack($"{Shared.Constants.AppName}DataV{Shared.Constants.DataVersion}");

            _ = new DynamoDbTables(stack);

            return app;
        }
    }
}