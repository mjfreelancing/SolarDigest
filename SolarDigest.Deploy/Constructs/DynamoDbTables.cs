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

        public DynamoDbTables(Stack stack)
            : base(stack, "DynamoDB")
        {
            ExceptionTable = CreateTable(stack, "Exception", false, StreamViewType.NEW_IMAGE, "TimeToLive");
            SiteTable = CreateTable(stack, "Site");
            EnergyCostsTable = CreateTable(stack, "EnergyCosts", true);

            PowerTable = CreateTable(stack, "Power", true, default, default, table =>
            {
                ConfigureReadAutoScaling(table);
                ConfigureWriteAutoScaling(table);
            });

            PowerMonthlyTable = CreateTable(stack, "PowerMonthly", true);
            PowerYearlyTable = CreateTable(stack, "PowerYearly", true);
            PowerUpdateHistoryTable = CreateTable(stack, "PowerUpdateHistory", true);
        }

        private ITable CreateTable(Stack stack, string tableName, bool hasSortKey = false, StreamViewType? streamViewType = default,
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

            stack.ExportValue(table.TableArn, new ExportValueOptions
            {
                Name = $"{stack.Region}_{stack.Account}_{tableName}Table"
            });

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