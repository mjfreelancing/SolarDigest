using Amazon.CDK;
using Amazon.CDK.AWS.Logs;
using SolarDigest.Shared.Utils;

namespace SolarDigest.Deploy.Constructs
{
    internal class LogGroups : Construct
    {
        internal LogGroup CatchAllLogGroup { get; }

        public LogGroups(Construct scope)
            : base(scope, "LogGroup")
        {
            CatchAllLogGroup = new LogGroup(this, "CatchAllLogGroup", new LogGroupProps
            {
                LogGroupName = $"{Helpers.GetAppVersionName()}-CatchAll",
                Retention = RetentionDays.ONE_WEEK
            });
        }
    }
}