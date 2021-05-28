using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema;
using SolarDigest.Deploy.Schema.Inputs;
using SolarDigest.Deploy.Schema.Types;

namespace SolarDigest.Deploy.Schema
{
    internal interface ISolarDigestMutationDefinition : IMutationDefinition
    {
        [GraphqlTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.AddSite)]
        Site AddSite([GraphqlTypeRequired] SiteInput site);

        [GraphqlTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.UpdateSite)]
        Site UpdateSite([GraphqlTypeRequired] string id, [GraphqlTypeRequired] SiteInput site, SiteTimestampsInput timestamps);

        //[GraphqlTypeRequired]
        //[LambdaDataSource(Constants.DataSource.CreateSite)]
        //Site AddSite2([GraphqlTypeRequired] SiteInput site);
    }
}