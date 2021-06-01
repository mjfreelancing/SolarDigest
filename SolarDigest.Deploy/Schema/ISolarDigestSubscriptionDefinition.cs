using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema;
using SolarDigest.Deploy.Schema.Types;

namespace SolarDigest.Deploy.Schema
{
    internal interface ISolarDigestSubscriptionDefinition : ISubscriptionDefinition
    {
        [SubscriptionMutation(nameof(ISolarDigestMutationDefinition.AddSite))]
        [GraphqlTypeRequired]
        ISite AddedSite(string id);
    }
}