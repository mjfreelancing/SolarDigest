using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Constructs;
using SolarDigest.Deploy.Extensions;
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

            var iam = new Iam(stack, apiProps.AppName);
            var tables = new DynamoDbTables(stack);
            var functions = new Functions(stack, apiProps, iam);
            var cloudWatch = new LogGroups(stack, apiProps);
            _ = new EventBridge(stack, apiProps, functions, cloudWatch);


            ConfigureTables(tables, functions);




            // builds a cache of roles to be associated with each lambda datasource
            //var dataSourceRoles = CreateDatasourceRoleCache(stack, iam);


            var authMode = new AuthorizationMode
            {
                AuthorizationType = AuthorizationType.API_KEY,
                //OpenIdConnectConfig = 
                //UserPoolConfig = 
            };



            _ = new AppSync(stack, apiProps, authMode/*, dataSourceRoles*/);




            app.Synth();
        }

        private static void ConfigureTables(DynamoDbTables tables, Functions functions)
        {
            tables.ExceptionTable.GrantStreamRead(functions.EmailExceptionFunction);

            tables.SiteTable.GrantReadDataToFunctions(
                functions.GetSiteInfoFunction,
                functions.HydrateAllSitesPowerFunction,
                functions.HydrateSitePowerFunction);

            tables.ExceptionTable.GrantWriteDataToFunctions(
                functions.GetSiteInfoFunction,
                functions.HydrateAllSitesPowerFunction,
                functions.HydrateSitePowerFunction);

            tables.ExceptionTable.AddEventSource(functions.EmailExceptionFunction);
        }


        //private static DataSourceRoleCache CreateDatasourceRoleCache(Stack stack, Iam iam)
        //{
        //    var serviceRole = new Role(stack, "ResolverServiceRole", new RoleProps
        //    {
        //        AssumedBy = new ServicePrincipal("appsync.amazonaws.com"),
        //        InlinePolicies = new Dictionary<string, PolicyDocument>
        //        {
        //            {"InvokeFunctionPolicy",  new PolicyDocument(new PolicyDocumentProps
        //            {
        //                Statements = new[] { iam.InvokeFunctionAccessPolicyStatement }
        //            })}
        //        }
        //    });

        //    var dataSourceRoles = new DataSourceRoleCache();

        //    dataSourceRoles.AddRole(
        //        serviceRole,
        //        Constants.ServiceName,
        //        Constants.DataSource.GetSite
        //        //Constants.DataSource.HydrateAllSitesPower, Constants.DataSource.HydrateSitePower, Constants.DataSource.EmailException
        //    );

        //    return dataSourceRoles;
        //}
    }
}
