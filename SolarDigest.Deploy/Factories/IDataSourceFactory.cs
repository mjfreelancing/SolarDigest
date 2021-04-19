using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Attributes;

namespace SolarDigest.Deploy.Factories
{
    internal interface IDataSourceFactory
    {
        BaseDataSource CreateDataSource(DataSourceAttribute attribute);
    }
}