﻿using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using SolarDigest.Shared.Utils;
using System;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace SolarDigest.Deploy.Constructs
{
    internal class DynamoDbTables : Construct
    {
        // To simplify stack creation, name these properties the same as the table name
        internal ITable Exception { get; }
        internal ITable Site { get; }
        internal ITable Power { get; }
        internal ITable PowerMonthly { get; }
        internal ITable PowerYearly { get; }
        internal ITable PowerUpdateHistory { get; }

        public DynamoDbTables(Construct scope)
            : base(scope, "DynamoDB")
        {
            Exception = CreateTable(nameof(Exception), false, StreamViewType.NEW_IMAGE, "TimeToLive");
            Site = CreateTable(nameof(Site));

            Power = CreateTable(nameof(Power), true, default, default, table =>
            {
                ConfigureReadAutoScaling(table);
                ConfigureWriteAutoScaling(table);
            });

            PowerMonthly = CreateTable(nameof(PowerMonthly), true, default, default, table =>
            {
                ConfigureReadAutoScaling(table);
                ConfigureWriteAutoScaling(table);
            });

            PowerYearly = CreateTable(nameof(PowerYearly), true, default, default, table =>
            {
                ConfigureReadAutoScaling(table);
                ConfigureWriteAutoScaling(table);
            });

            PowerUpdateHistory = CreateTable(nameof(PowerUpdateHistory), true, default, "TimeToLive");
        }

        public static string GetExportTableName(string tableName)
        {
            return $"{Helpers.GetAppVersionName()}-Data-Table-{tableName}";
        }

        public static string GetExportStreamName(string tableName)
        {
            return $"{Helpers.GetAppVersionName()}Data-Stream-{tableName}";
        }

        private ITable CreateTable(string tableName, bool hasSortKey = false, StreamViewType? streamViewType = default,
            string ttlAttribute = default, Action<Table> configAction = default)
        {
            var table = new Table(this, tableName, new TableProps
            {
                TableName = $"{Helpers.GetAppVersionName()}_{tableName}",
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                SortKey = hasSortKey ? new Attribute { Name = "Sort", Type = AttributeType.STRING } : default,
                Stream = streamViewType,
                TimeToLiveAttribute = ttlAttribute
            });

            configAction?.Invoke(table);

            // must pass tableName since table.TableName will be a token during CDK deployment
            ExportTableArn(table, tableName);

            return table;
        }

        private static void ConfigureReadAutoScaling(Table table, double minCapacity = 5, double maxCapacity = 25, double utilizationPercent = 60)
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

        private static void ConfigureWriteAutoScaling(Table table, double minCapacity = 5, double maxCapacity = 25, double utilizationPercent = 60)
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

        private void ExportTableArn(ITable table, string tableName)
        {
            var stack = Stack.Of(this);

            stack.ExportValue(table.TableArn, new ExportValueOptions
            {
                Name = GetExportTableName(tableName)
            });

            if (table.TableStreamArn != null)
            {
                stack.ExportValue(table.TableStreamArn, new ExportValueOptions
                {
                    Name = GetExportStreamName(tableName)
                });
            }
        }
    }
}