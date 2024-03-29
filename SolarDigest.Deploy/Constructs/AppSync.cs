﻿using AllOverIt.Aws.Cdk.AppSync.MappingTemplates;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Schema;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class AppSync : Construct
    {
        public AppSync(Construct scope, AuthorizationMode authMode, IMappingTemplates mappingTemplates)
            : base(scope, "AppSync")
        {
            var graphql = new SolarDigestGraphql(this, authMode, mappingTemplates);

            graphql
                .AddSchemaQuery<ISolarDigestQueryDefinition>()
                .AddSchemaMutation<ISolarDigestMutationDefinition>();
                //.AddSchemaSubscription<ISolarDigestSubscriptionDefinition>();
        }
    }
}