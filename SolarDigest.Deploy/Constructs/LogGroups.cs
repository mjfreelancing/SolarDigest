using Amazon.CDK;
using Amazon.CDK.AWS.Logs;

namespace SolarDigest.Deploy.Constructs
{
    internal class LogGroups : Construct
    {
        internal LogGroup CatchAllLogGroup { get; }

        public LogGroups(Construct scope, SolarDigestAppProps appProps)
            : base(scope, "LogGroup")
        {
            CatchAllLogGroup = new LogGroup(this, "CatchAllLogGroup", new LogGroupProps
            {
                LogGroupName = $"{appProps.AppName}_CatchAll",
                Retention = RetentionDays.ONE_WEEK
            });
        }
    }
}