﻿using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "PowerConnection")]
    internal interface IPowerConnection
    {
        [SchemaTypeRequired]
        public ITimeWatts[] TimeWatts { get; }

        public string NextToken { get; }
    }
}