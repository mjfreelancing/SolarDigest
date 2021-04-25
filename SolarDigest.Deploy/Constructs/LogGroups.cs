using Amazon.CDK;
using Amazon.CDK.AWS.Logs;

namespace SolarDigest.Deploy.Constructs
{
    internal class LogGroups : Construct
    {
        internal LogGroup CatchAllLogGroup { get; }

        public LogGroups(Construct scope, SolarDigestApiProps apiProps)
            : base(scope, "LogGroup")
        {
            CatchAllLogGroup = new LogGroup(this, "CatchAllLogGroup", new LogGroupProps
            {
                LogGroupName = $"{apiProps.AppName}_CatchAll"
            });
        }
    }
}