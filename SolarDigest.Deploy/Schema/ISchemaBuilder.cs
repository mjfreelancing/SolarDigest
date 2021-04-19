using SolarDigest.Deploy.Schema.Mutation;
using SolarDigest.Deploy.Schema.Query;
using SolarDigest.Deploy.Schema.Subscription;

namespace SolarDigest.Deploy.Schema
{
    internal interface ISchemaBuilder
    {
        ISchemaBuilder AddQuery<TType>() where TType : IQueryDefinition;
        ISchemaBuilder AddMutation<TType>() where TType : IMutationDefinition;
        ISchemaBuilder AddSubscription<TType>() where TType : ISubscriptionDefinition;
    }
}