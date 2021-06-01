using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema;
using SolarDigest.Deploy.Schema.Inputs;
using SolarDigest.Deploy.Schema.Types;

namespace SolarDigest.Deploy.Schema
{
    internal interface ISolarDigestMutationDefinition : IMutationDefinition
    {
        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.AddSite)]
        ISite AddSite([SchemaTypeRequired] ISiteInput site);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.UpdateSite)]
        ISite UpdateSite([SchemaTypeRequired] string id, [SchemaTypeRequired] ISiteInput site, ISiteTimestampsInput timestamps);

        //[GraphqlTypeRequired]
        //[LambdaDataSource(Constants.DataSource.CreateSite)]
        //Site AddSite2([GraphqlTypeRequired] SiteInput site);
    }
}