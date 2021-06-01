using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [GraphqlSchemaType(GraphqlSchemaType.Type, "TimeWatts")]
    internal interface ITimeWatts
    {
        [GraphqlTypeRequired]
        public string Time { get; }

        [GraphqlTypeRequired]
        public double Watts { get; }

        [GraphqlTypeRequired]
        public float WattHour { get; }
    }
}