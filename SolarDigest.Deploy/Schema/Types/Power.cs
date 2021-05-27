using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    internal interface Power
    {
        [GraphqlTypeRequired]
        public TimeWatts[] Watts { get; }
    }
}