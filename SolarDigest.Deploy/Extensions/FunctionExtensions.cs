using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using SolarDigest.Deploy.Constructs;

namespace SolarDigest.Deploy.Extensions
{
    internal static class FunctionExtensions
    {
        public static IFunction AddPolicyStatements(this IFunction function, params PolicyStatement[] statements)
        {
            foreach (var statement in statements)
            {
                function.AddToRolePolicy(statement);
            }

            return function;
        }

        public static IFunction GrantDescribeTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoDescribeTablePolicy(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantReadTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoReadDataPolicy(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantWriteTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoWriteDataPolicy(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantReadWriteTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            function.GrantReadTableData(iam, tableNames);
            function.GrantWriteTableData(iam, tableNames);

            return function;
        }

        public static IFunction GrantStreamReadData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement  = iam.GetDynamoStreamReadPolicy(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction AddEventSource(this IFunction function, ITable table)
        {
            var exceptionTableEventSource = new DynamoEventSource(table, new DynamoEventSourceProps
            {
                StartingPosition = StartingPosition.LATEST,
                RetryAttempts = 0
            });

            function.AddEventSource(exceptionTableEventSource);

            return function;
        }
    }
}