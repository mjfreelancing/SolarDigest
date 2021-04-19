using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.IAM;
using SolarDigest.Deploy.Factories;
using SolarDigest.Deploy.Schema;
using SolarDigest.Deploy.Schema.Mutation;
using SolarDigest.Deploy.Schema.Query;
using SolarDigest.Deploy.Schema.Subscription;
using System.Collections.Generic;
using System.Text;
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
                Constants.DataSource.GetSiteInfo
                //Constants.DataSource.HydrateAllSitesPower, Constants.DataSource.HydrateSitePower, Constants.DataSource.EmailException
            );

            _ = new PublicApiConstruct(stack, apiProps, authMode, dataSourceRoles);




            app.Synth();

        }
    }

    internal sealed class IamConstruct : Construct
    {
        internal PolicyStatement InvokeFunctionAccessPolicyStatement { get; }

        public IamConstruct(Construct stack, string appName)
            : base(stack, $"{appName}IAM")
        {
            InvokeFunctionAccessPolicyStatement = CreateInvokeFunctionAccessPolicyStatement(appName);
        }

        private PolicyStatement CreateInvokeFunctionAccessPolicyStatement(string appName)
        {
            var stack = Stack.Of(this);

            return new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "lambda:InvokeFunction"
                },
                Resources = new[]
                {
                    // wildcard to all functions and logs for this account / app
                    $"arn:aws:lambda:{stack.Region}:{stack.Account}:function:{appName}DataProcedure"
                }
            });
        }
    }

    internal sealed class SolarDigestGraphqlApi : GraphqlApi
    {
        private readonly ISchemaBuilder _schemaBuilder;

        public SolarDigestGraphqlApi(Construct scope, SolarDigestApiProps apiProps, IAuthorizationMode authMode, IDataSourceRoleCache serviceRoles)
            : base(scope, "Graphql", new GraphqlApiProps
            {
                Name = $"{apiProps.AppName} V{apiProps.Version}",
                AuthorizationConfig = new AuthorizationConfig { DefaultAuthorization = authMode }
            })
        {
            // these require the GraphqlApi reference
            var dataSourceFactory = new DataSourceFactory(this, apiProps, serviceRoles);
            var resolverFactory = new ResolverFactory(this, apiProps, dataSourceFactory);
            var gqlTypeCache = new GraphqlTypeStore(resolverFactory);
            _schemaBuilder = new SchemaBuilder(this, apiProps, gqlTypeCache, dataSourceFactory);
        }

        public SolarDigestGraphqlApi AddSchemaQuery<TType>() where TType : IQueryDefinition
        {
            _schemaBuilder.AddQuery<TType>();
            return this;
        }

        public SolarDigestGraphqlApi AddSchemaMutation<TType>() where TType : IMutationDefinition
        {
            _schemaBuilder.AddMutation<TType>();
            return this;
        }

        public SolarDigestGraphqlApi AddSchemaSubscription<TType>() where TType : ISubscriptionDefinition
        {
            _schemaBuilder.AddSubscription<TType>();
            return this;
        }
    }

    internal abstract class ApiConstructBase : Construct
    {
        protected SolarDigestGraphqlApi SolarDigestGraphqlApi { get; }

        protected ApiConstructBase(Construct scope, SolarDigestApiProps apiProps, AuthorizationMode authMode, IDataSourceRoleCache dataSourceRoleCache)
            : base(scope, "Api")
        {
            // automatically adds all required types to the schema
            SolarDigestGraphqlApi = new SolarDigestGraphqlApi(this, apiProps, authMode, dataSourceRoleCache);
        }
    }

    internal sealed class PublicApiConstruct : ApiConstructBase
    {
        public PublicApiConstruct(Construct scope, SolarDigestApiProps apiProps, AuthorizationMode authMode, IDataSourceRoleCache dataSourceRoleCache)
            : base(scope, apiProps, authMode, dataSourceRoleCache)
        {
            SolarDigestGraphqlApi
                .AddSchemaQuery<IPublicQueryDefinition>();
                //.AddSchemaMutation<IPublicMutationDefinition>();
        }
    }

    internal static class Constants
    {
        internal const int ApiVersion = 1;
        internal const string ServiceName = "SolarDigest";

        internal static class DataSource
        {
            internal const string GetSiteInfo = "GetSiteInfo";
            internal const string HydrateAllSitesPower = "HydrateAllSitesPower";
            internal const string HydrateSitePower = "HydrateSitePower";
            internal const string EmailException = "EmailException";
        }
    }

    internal interface IMappingTemplates
    {
        string RequestMapping { get; }
        string ResponseMapping { get; }
    }

    internal sealed class SolarDigestApiProps
    {
        public string AppName => Constants.ServiceName;
        public int Version { get; set; }
        public IMappingTemplates MappingTemplates { get; set; }     // currently assumed to be all the same
    }

    internal sealed class SolarDigestMappingTemplates : IMappingTemplates
    {
        private string _requestMapping;
        private string _responseMapping;

        public string RequestMapping
        {
            get
            {
                _requestMapping ??= CreateTemplate(
                    "{",
                    @"  ""version"" : ""2017-02-28"",",
                    @"  ""operation"": ""Invoke"",",
                    @"  ""payload"": {",
                    @"    ""username"": $utils.toJson($ctx.identity.claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']),",
                    @"    ""source"": ""$utils.escapeJavaScript($utils.toJson($ctx.source))"",",
                    @"    ""arguments"": ""$utils.escapeJavaScript($utils.toJson($ctx.arguments))""",
                    "    }",
                    @"}"
                );

                return _requestMapping;
            }
        }

        public string ResponseMapping
        {
            get
            {
                _responseMapping ??= CreateTemplate(
                    "#if($ctx.error)",
                    "  $util.error($ctx.error.message, $ctx.error.type)",
                    "#end",
                    "",
                    @"#if($ctx.result.Status == ""Success"")",
                    "  $ctx.result.Payload",
                    @"#elseif($ctx.result.Status == ""ValidationError"")",
                    "  $util.error($ctx.result.ValidationErrors, $ctx.result.Status)",
                    "#else",
                    "  $util.error($ctx.result.Status, $ctx.result.Status)",
                    "#end"
                );

                return _responseMapping;
            }
        }


        private static string CreateTemplate(params string[] lines)
        {
            var builder = new StringBuilder();

            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }
    }
}
