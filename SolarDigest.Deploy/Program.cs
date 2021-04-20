using AllOverIt.Aws.Cdk.AppSync;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.IAM;
using System.Collections.Generic;
using Environment = Amazon.CDK.Environment;
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

            var stack = new Stack(app, $"{apiProps.AppName}V{apiProps.Version}", new StackProps
            {
                Description = $"Creates all resources for the {apiProps.AppName} API",
                Env = new Environment
                {
                    Account = SystemEnvironment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = SystemEnvironment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
                }
            });

            var iam = new IamConstruct(stack, apiProps.AppName);

            var serviceRole = new Role(stack, "ResolverServiceRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("appsync.amazonaws.com"),
                InlinePolicies = new Dictionary<string, PolicyDocument>
                {
                    {"InvokeFunctionPolicy",  new PolicyDocument(new PolicyDocumentProps
                    {
                        Statements = new[] { iam.InvokeFunctionAccessPolicyStatement }
                    })}
                }
            });


            var authMode = new AuthorizationMode
            {
                AuthorizationType = AuthorizationType.API_KEY,
                //OpenIdConnectConfig = 
                //UserPoolConfig = 
            };


            var dataSourceRoles = new DataSourceRoleCache();

            dataSourceRoles.AddRole(
                serviceRole,
                Constants.ServiceName,
                Constants.DataSource.GetSite
                //Constants.DataSource.HydrateAllSitesPower, Constants.DataSource.HydrateSitePower, Constants.DataSource.EmailException
            );

            _ = new SolarDigestApiConstruct(stack, apiProps, authMode, dataSourceRoles);




            app.Synth();

        }
    }
}
