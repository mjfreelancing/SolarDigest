using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema;
using SolarDigest.Deploy.Schema.Types;

namespace SolarDigest.Deploy.Schema
{
    internal interface ISolarDigestQueryDefinition : IQueryDefinition
    {
        [GraphqlTypeRequired]
        [LambdaDataSource(Constants.ServiceName, Constants.Function.GetSiteInfo)]
        Site Site([GraphqlTypeRequired] int id);

        //[LambdaDataSource(Constants.ServiceName, Constants.DataSource.HydrateAllSitesPower)]
        //Site HydrateAllSitesPower();

        //[LambdaDataSource(Constants.ServiceName, Constants.DataSource.HydrateSitePower)]
        //Site User([GraphqlTypeRequired] int id);

        //[LambdaDataSource(Constants.ServiceName, Constants.DataSource.EmailException)]
        //Site EmailException([GraphqlTypeRequired] int id);
    }
}