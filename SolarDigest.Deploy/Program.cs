using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Constructs;
using System;
using Environment = Amazon.CDK.Environment;
using Stack = Amazon.CDK.Stack;
using SystemEnvironment = System.Environment;

namespace SolarDigest.Deploy
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();

            var apiProps = new SolarDigestApiProps
            {
                Version = Constants.ApiVersion,
                MappingTemplates = new SolarDigestMappingTemplates()
            };

            var stack = new Stack(app, $"{apiProps.AppName}v{apiProps.Version}", new StackProps
            {
                Description = $"Creates all resources for the {apiProps.AppName} API",
                Env = new Environment
                {
                    Account = SystemEnvironment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = SystemEnvironment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
                }
            });

            var iam = new Iam(stack, apiProps.AppName);
            var tables = new DynamoDbTables(stack);
            var functions = new Functions(stack, apiProps, iam, tables);
            var cloudWatch = new LogGroups(stack, apiProps);
            _ = new EventBridge(stack, apiProps, functions, cloudWatch);

            var authMode = new AuthorizationMode
            {
                AuthorizationType = AuthorizationType.API_KEY,
                ApiKeyConfig = new ApiKeyConfig
                {
                    Expires = Expiration.AtDate(DateTime.Now.AddDays(365))
                }
                //OpenIdConnectConfig = 
                //UserPoolConfig = 
            };

            _ = new AppSync(stack, apiProps, authMode);

            app.Synth();
        }
    }
}
