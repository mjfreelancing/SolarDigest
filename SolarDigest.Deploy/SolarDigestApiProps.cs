using AllOverIt.Aws.Cdk.AppSync;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestApiProps
    {
        public string AppName => Constants.ServiceName;
        public int Version { get; set; }
        public IMappingTemplates MappingTemplates { get; set; }     // currently assumed to be all the same
    }
}