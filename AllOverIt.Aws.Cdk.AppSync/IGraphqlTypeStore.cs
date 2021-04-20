using Amazon.CDK.AWS.AppSync;
using System;
using SystemType = System.Type;

namespace AllOverIt.Aws.Cdk.AppSync
{
    public interface IGraphqlTypeStore
    {
        GraphqlType GetGraphqlType(SystemType type, bool isRequired, bool isList, bool isRequiredList, Action<IIntermediateType> typeCreated);
    }
}