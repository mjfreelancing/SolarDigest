﻿using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema.Types;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType("PowerEdge", GraphqlSchemaType.Type)]
    internal interface IPowerEdge : IEdge<ITimeWatts>
    {
    }
}