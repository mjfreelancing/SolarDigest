using AllOverIt.Extensions;
using AllOverIt.Helpers;
using SolarDigest.Deploy.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Deploy.Attributes
{
    // Used to indicate which mutations will trigger the subscription notification
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class SubscriptionMutationAttribute : Attribute
    {
        public IEnumerable<string> Mutations { get; }

        public SubscriptionMutationAttribute(params string[] mutations)
        {
            Mutations = mutations.WhenNotNullOrEmpty(nameof(mutations))
                .Select(item => item.GetGraphqlName())
                .AsReadOnlyCollection();
        }
    }
}