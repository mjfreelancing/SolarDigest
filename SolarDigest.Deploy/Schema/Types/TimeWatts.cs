using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    internal interface TimeWatts
    {
        [GraphqlTypeRequired]
        public string Time { get; }

        [GraphqlTypeRequired]
        public double Watts { get; }

        [GraphqlTypeRequired]
        public float WattHour { get; }
    }
}