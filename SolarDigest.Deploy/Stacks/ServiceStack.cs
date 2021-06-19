using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Constructs;
using SolarDigest.Deploy.Extensions;
using System;

namespace SolarDigest.Deploy.Stacks
{
    // When deleting this stack, need to manually delete:
    //  * log groups
    //  * solardigest-uploads and solardigest-downloads S3 buckets
    //
    internal sealed class ServiceStack
    {
        public static App CreateApp()
        {
            var app = new App();

            var apiProps = new SolarDigestAppProps
            {
                StackName = $"{Constants.AppName}Service",
                AppName = Constants.AppName,
                Version = Constants.ServiceVersion,
            };

            var stack = app.CreateRootStack(apiProps);

            var iam = new Iam(stack, apiProps);

            _ = new S3Buckets(stack);

            var users = new Users(stack,  iam);

            _ = new ParameterStore(stack, users);

            var mappingTemplates = new SolarDigestMappingTemplates();

            var functions = new Functions(stack, apiProps, iam, mappingTemplates);

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

            _ = new AppSync(stack, apiProps, authMode, mappingTemplates);

            return app;
        }
    }
}