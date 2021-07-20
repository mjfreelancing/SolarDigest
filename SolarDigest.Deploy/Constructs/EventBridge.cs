using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;

namespace SolarDigest.Deploy.Constructs
{
    internal class EventBridge : Construct
    {
        private enum SolarEdgeEventType
        {
            // All events
            CatchAll,

            // A scheduled events
            HydrateAllSitesPower,
            AggregateAllSitesPower,
            EmailAllSitesUpdateHistory,

            // Events posted from functions
            HydrateSitePowerEvent,
            AggregateSitePowerEvent
        }

        public EventBridge(Construct scope, Functions functions, LogGroups logGroups)
            : base(scope, "EventBridge")
        {
            CreateCatchAll(logGroups.CatchAllLogGroup);
            CreateHydrateSitePower(functions.HydrateSitePowerFunction);
            CreateHydrateAllSitesPowerFunction(functions.HydrateAllSitesPowerFunction);
            CreateAggregateSitePower(functions.AggregateSitePowerFunction);
            CreateAggregateAllSitesPower(functions.AggregateAllSitesPowerFunction);
            CreateEmailAllSitesUpdateHistory(functions.EmailSiteUpdateHistoryFunction);
        }

        // Logs all events received for this account
        private void CreateCatchAll(ILogGroup catchAllLogGroup)
        {
            var stack = Stack.Of(this);
            var catchAllLogGroupTarget = new CloudWatchLogGroup(catchAllLogGroup);

            _ = new Rule(this, $"{SolarEdgeEventType.CatchAll}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{Shared.Helpers.GetAppVersionName()}_{SolarEdgeEventType.CatchAll}",
                Description = "Log all received events",
                EventPattern = new EventPattern
                {
                    Account = new[] { stack.Account }
                },
                Targets = new IRuleTarget[] { catchAllLogGroupTarget }
            });
        }

        // calls the target function when a HydrateSitePowerEvent message is received
        private void CreateHydrateSitePower(IFunction targetFunction)
        {
            _ = new Rule(this, $"{SolarEdgeEventType.HydrateSitePowerEvent}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{Shared.Helpers.GetAppVersionName()}_{SolarEdgeEventType.HydrateSitePowerEvent}",
                Description = "Hydrates power data for a given site",
                EventPattern = new EventPattern
                {
                    DetailType = new[] { $"{SolarEdgeEventType.HydrateSitePowerEvent}" }
                },
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }

        // calls the target function once per hour, at the top of the hour
        private void CreateHydrateAllSitesPowerFunction(IFunction targetFunction)
        {
            _ = new Rule(this, $"{SolarEdgeEventType.HydrateAllSitesPower}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{Shared.Helpers.GetAppVersionName()}_{SolarEdgeEventType.HydrateAllSitesPower}",
                Description = "Hydrates power data for all sites every hour",
                Schedule = Schedule.Cron(new CronOptions { Minute = "0" }),
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });

        }

        // calls the target function when a AggregateSitePowerEvent message is received
        private void CreateAggregateSitePower(IFunction targetFunction)
        {
            _ = new Rule(this, $"{SolarEdgeEventType.AggregateSitePowerEvent}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{Shared.Helpers.GetAppVersionName()}_{SolarEdgeEventType.AggregateSitePowerEvent}",
                Description = "Aggregates power data for a given site",
                EventPattern = new EventPattern
                {
                    DetailType = new[] { $"{SolarEdgeEventType.AggregateSitePowerEvent}" }
                },
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }

        // calls the target function once per hour, at the top of the hour
        private void CreateAggregateAllSitesPower(IFunction targetFunction)
        {
            _ = new Rule(this, $"{Shared.Helpers.GetAppVersionName()}_{SolarEdgeEventType.AggregateAllSitesPower}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{Shared.Helpers.GetAppVersionName()}_{SolarEdgeEventType.AggregateAllSitesPower}",
                Description = "Aggregates power data for all sites every hour",
                Schedule = Schedule.Cron(new CronOptions { Minute = "0" }),
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }

        // calls the target function once per hour, at the top of the hour
        private void CreateEmailAllSitesUpdateHistory(IFunction targetFunction)
        {
            _ = new Rule(this, $"{SolarEdgeEventType.EmailAllSitesUpdateHistory}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{Shared.Helpers.GetAppVersionName()}_{SolarEdgeEventType.EmailAllSitesUpdateHistory}",
                Description = "Sends all sites a status update via email",
                Schedule = Schedule.Cron(new CronOptions { Minute = "0" }),
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }
    }
}