﻿using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.MappingTemplates;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestGraphql : AppGraphqlBase
    {
        public SolarDigestGraphql(Construct scope, SolarDigestAppProps appProps, IAuthorizationMode authMode, IMappingTemplates mappingTemplates)
            : base(scope, "GraphQl", new GraphqlApiProps
            {
                Name = $"{appProps.AppName} V{appProps.Version}",
                AuthorizationConfig = new AuthorizationConfig { DefaultAuthorization = authMode }
            }, mappingTemplates)
        {
        }
    }
}