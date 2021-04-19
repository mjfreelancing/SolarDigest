using SolarDigest.Deploy.Attributes;
using SolarDigest.Deploy.Schema.Types;

namespace SolarDigest.Deploy.Schema.Query
{
    internal interface IPublicQueryDefinition : IQueryDefinition
    {
        [GraphqlTypeRequired]
        [LambdaDataSource(Constants.ServiceName, Constants.DataSource.GetSiteInfo)]
        Site Site([GraphqlTypeRequired] int id);

        //[LambdaDataSource(Constants.ServiceName, Constants.DataSource.HydrateAllSitesPower)]
        //Site HydrateAllSitesPower();

        //[LambdaDataSource(Constants.ServiceName, Constants.DataSource.HydrateSitePower)]
        //Site User([GraphqlTypeRequired] int id);

        //[LambdaDataSource(Constants.ServiceName, Constants.DataSource.EmailException)]
        //Site EmailException([GraphqlTypeRequired] int id);
    }
}