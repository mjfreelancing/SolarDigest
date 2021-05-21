using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using System;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace SolarDigest.Deploy.Constructs
{
    internal class DynamoDbTables : Construct
    {
        internal ITable ExceptionTable { get; }
        internal ITable SiteTable { get; }
        internal ITable EnergyCostsTable { get; }
        internal ITable PowerTable { get; }
        internal ITable PowerMonthlyTable { get; }
        internal ITable PowerYearlyTable { get; }
        internal ITable PowerUpdateHistoryTable { get; }

        public DynamoDbTables(Construct scope)
            : base(scope, "DynamoDB")
        {
            ExceptionTable = CreateTable("Exception", false, StreamViewType.NEW_IMAGE, "TimeToLive");
            SiteTable = CreateTable("Site");
            EnergyCostsTable = CreateTable("EnergyCosts", true);

            PowerTable = CreateTable("Power", true, default, default, table =>
            {
                ConfigureReadAutoScaling(table);
                ConfigureWriteAutoScaling(table);
            });

            PowerMonthlyTable = CreateTable("PowerMonthly", true);
            PowerYearlyTable = CreateTable("PowerYearly", true);
            PowerUpdateHistoryTable = CreateTable("PowerUpdateHistory", true);
        }

        private ITable CreateTable(string tableName, bool hasSortKey = false, StreamViewType? streamViewType = default,
            string ttlAttribute = default, Action<Table> configAction = default)
        {
            var table = new Table(this, tableName, new TableProps
            {
                TableName = tableName,
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                SortKey = hasSortKey ? new Attribute { Name = "Sort", Type = AttributeType.STRING } : default,
                Stream = streamViewType,
                TimeToLiveAttribute = ttlAttribute
            });

            configAction?.Invoke(table);

            return table;
        }

        private static void ConfigureReadAutoScaling(Table table, double minCapacity = 5, double maxCapacity = 25, double utilizationPercent = 80)
        {
            table
                .AutoScaleReadCapacity(new EnableScalingProps
                {
                    MinCapacity = minCapacity,
                    MaxCapacity = maxCapacity
                })
                .ScaleOnUtilization(new UtilizationScalingProps
                {
                    TargetUtilizationPercent = utilizationPercent

                    // The default is 0 seconds for DynamoDb
                    // ScaleInCooldown = Duration.Seconds(60),
                    // ScaleOutCooldown = Duration.Seconds(60)
                });
        }

        private static void ConfigureWriteAutoScaling(Table table, double minCapacity = 5, double maxCapacity = 25, double utilizationPercent = 80)
        {
            table
                .AutoScaleWriteCapacity(new EnableScalingProps
                {
                    MinCapacity = minCapacity,
                    MaxCapacity = maxCapacity
                })
                .ScaleOnUtilization(new UtilizationScalingProps
                {
                    TargetUtilizationPercent = utilizationPercent

                    // The default is 0 seconds for DynamoDb
                    // ScaleInCooldown = Duration.Seconds(60),
                    // ScaleOutCooldown = Duration.Seconds(60)
                });
        }
    }
}