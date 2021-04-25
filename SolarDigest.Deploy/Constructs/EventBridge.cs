using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;

namespace SolarDigest.Deploy.Constructs
{
    internal class EventBridge : Construct
    {
        private enum TriggeredEventType
        {
            HydrateSitePowerEvent
        }

        private enum ScheduledEventType
        {
            HydrateAllSitesPower,
        }

        public EventBridge(Construct scope, SolarDigestApiProps apiProps, Functions functions, LogGroups logGroups)
            : base(scope, "EventBridge")
        {
            var appName = apiProps.AppName;

            CreateTriggerCatchAll(appName, logGroups.CatchAllLogGroup);
            CreateTriggerHydrateSitePower(appName, functions.HydrateSitePowerFunction);
            CreateScheduleHydrateAllSitesPowerFunction(appName, functions.HydrateAllSitesPowerFunction);
        }

        private void CreateTriggerCatchAll(string appName, ILogGroup catchAllLogGroup)
        {
            var stack = Stack.Of(this);
            var catchAllLogGroupTarget = new CloudWatchLogGroup(catchAllLogGroup);

            _ = new Rule(this, $"{appName}_Triggered_CatchAll", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_Triggered_CatchAll",
                Description = "Log all received events",
                EventPattern = new EventPattern
                {
                    Account = new[] { stack.Account }
                },
                Targets = new IRuleTarget[] { catchAllLogGroupTarget }
            });
        }

        private void CreateTriggerHydrateSitePower(string appName, IFunction targetFunction)
        {
            _ = new Rule(this, $"Triggered{TriggeredEventType.HydrateSitePowerEvent}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_Triggered_{TriggeredEventType.HydrateSitePowerEvent}",
                Description = "Hydrates power data for a given site",
                EventPattern = new EventPattern
                {
                    DetailType = new[] { $"{TriggeredEventType.HydrateSitePowerEvent}" }
                },
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }

        private void CreateScheduleHydrateAllSitesPowerFunction(string appName, IFunction targetFunction)
        {
            _ = new Rule(this, $"Scheduled{ScheduledEventType.HydrateAllSitesPower}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_Scheduled_{ScheduledEventType.HydrateAllSitesPower}",
                Description = "Hydrates power data for all sites every hour",
                Schedule = Schedule.Cron(new CronOptions { Minute = "0" }),
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });

        }
    }
}