﻿using Amazon.CDK;
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

        public EventBridge(Construct scope, SolarDigestAppProps appProps, Functions functions, LogGroups logGroups)
            : base(scope, "EventBridge")
        {
            var appName = appProps.AppName;

            CreateCatchAll(appName, logGroups.CatchAllLogGroup);
            CreateHydrateSitePower(appName, functions.HydrateSitePowerFunction);
            CreateHydrateAllSitesPowerFunction(appName, functions.HydrateAllSitesPowerFunction);
            CreateAggregateSitePower(appName, functions.AggregateSitePowerFunction);
            CreateAggregateAllSitesPower(appName, functions.AggregateAllSitesPowerFunction);
            CreateEmailAllSitesUpdateHistory(appName, functions.EmailSiteUpdateHistoryFunction);
        }

        // Logs all events received for this account
        private void CreateCatchAll(string appName, ILogGroup catchAllLogGroup)
        {
            var stack = Stack.Of(this);
            var catchAllLogGroupTarget = new CloudWatchLogGroup(catchAllLogGroup);

            _ = new Rule(this, $"{appName}_{SolarEdgeEventType.CatchAll}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_{SolarEdgeEventType.CatchAll}",
                Description = "Log all received events",
                EventPattern = new EventPattern
                {
                    Account = new[] { stack.Account }
                },
                Targets = new IRuleTarget[] { catchAllLogGroupTarget }
            });
        }

        // calls the target function when a HydrateSitePowerEvent message is received
        private void CreateHydrateSitePower(string appName, IFunction targetFunction)
        {
            _ = new Rule(this, $"{appName}_{SolarEdgeEventType.HydrateSitePowerEvent}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_{SolarEdgeEventType.HydrateSitePowerEvent}",
                Description = "Hydrates power data for a given site",
                EventPattern = new EventPattern
                {
                    DetailType = new[] { $"{SolarEdgeEventType.HydrateSitePowerEvent}" }
                },
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }

        // calls the target function once per hour, at the top of the hour
        private void CreateHydrateAllSitesPowerFunction(string appName, IFunction targetFunction)
        {
            _ = new Rule(this, $"{appName}_{SolarEdgeEventType.HydrateAllSitesPower}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_{SolarEdgeEventType.HydrateAllSitesPower}",
                Description = "Hydrates power data for all sites every hour",
                Schedule = Schedule.Cron(new CronOptions { Minute = "0" }),
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });

        }

        // calls the target function when a AggregateSitePowerEvent message is received
        private void CreateAggregateSitePower(string appName, IFunction targetFunction)
        {
            _ = new Rule(this, $"{appName}_{SolarEdgeEventType.AggregateSitePowerEvent}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_{SolarEdgeEventType.AggregateSitePowerEvent}",
                Description = "Aggregates power data for a given site",
                EventPattern = new EventPattern
                {
                    DetailType = new[] { $"{SolarEdgeEventType.AggregateSitePowerEvent}" }
                },
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }

        // calls the target function once per hour, at the top of the hour
        private void CreateAggregateAllSitesPower(string appName, IFunction targetFunction)
        {
            _ = new Rule(this, $"{appName}_{SolarEdgeEventType.AggregateAllSitesPower}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_{SolarEdgeEventType.AggregateAllSitesPower}",
                Description = "Aggregates power data for all sites every hour",
                Schedule = Schedule.Cron(new CronOptions { Minute = "0" }),
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }

        // calls the target function once per hour, at the top of the hour
        private void CreateEmailAllSitesUpdateHistory(string appName, IFunction targetFunction)
        {
            _ = new Rule(this, $"{appName}_{SolarEdgeEventType.EmailAllSitesUpdateHistory}", new RuleProps
            {
                // using the default EventBus
                RuleName = $"{appName}_{SolarEdgeEventType.EmailAllSitesUpdateHistory}",
                Description = "Sends all sites a status update via email",
                Schedule = Schedule.Cron(new CronOptions { Minute = "0" }),
                Targets = new IRuleTarget[] { new LambdaFunction(targetFunction) }
            });
        }
    }
}