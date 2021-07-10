using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType("TimeWatts", GraphqlSchemaType.Type)]
    internal interface ITimeWatts
    {
        [SchemaTypeRequired]
        public string Time { get; }

        [SchemaTypeRequired]
        public double Watts { get; }

        [SchemaTypeRequired]
        public float WattHour { get; }
    }
}