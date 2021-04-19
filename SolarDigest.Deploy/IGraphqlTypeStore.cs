using Amazon.CDK.AWS.AppSync;
using System;
using SystemType = System.Type;

namespace SolarDigest.Deploy
{
    internal interface IGraphqlTypeStore
    {
        GraphqlType GetGraphqlType(SystemType type, bool isRequired, bool isList, bool isRequiredList, Action<IIntermediateType> typeCreated);
    }
}