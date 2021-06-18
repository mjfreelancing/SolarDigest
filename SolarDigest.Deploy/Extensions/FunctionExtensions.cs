using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using SolarDigest.Deploy.Constructs;

namespace SolarDigest.Deploy.Extensions
{
    internal static class FunctionExtensions
    {
        //public static IFunction AddPolicyStatements(this IFunction function, params PolicyStatement[] statements)
        //{
        //    foreach (var statement in statements)
        //    {
        //        function.AddToRolePolicy(statement);
        //    }

        //    return function;
        //}

        public static IFunction GrantDescribeTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoDescribeTablePolicyStatement(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantReadTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoReadDataPolicyStatement(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantWriteTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoWriteDataPolicyStatement(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantReadWriteTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            function.GrantReadTableData(iam, tableNames);
            function.GrantWriteTableData(iam, tableNames);

            return function;
        }

        public static IFunction GrantBatchWriteTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoBatchWriteTablePolicyStatement(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantQueryTableData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement = iam.GetDynamoQueryTablePolicyStatement(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantStreamReadData(this IFunction function, Iam iam, params string[] tableNames)
        {
            var statement  = iam.GetDynamoStreamReadPolicyStatement(tableNames);
            function.AddToRolePolicy(statement);

            return function;
        }

        public static IFunction GrantSendEmail(this IFunction function, Iam iam)
        {
            function.AddToRolePolicy(iam.SendEmailPolicyStatement);

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

        public static IFunction GrantPutDefaultEventBridgeEvents(this IFunction function, Iam iam)
        {
            function.AddToRolePolicy(iam.PutDefaultEventBridgeEventsPolicyStatement);

            return function;
        }

        public static IFunction GrantGetParameters(this IFunction function, Iam iam, string path)
        {
            function.AddToRolePolicy(iam.GetParameterPolicyStatement(path));

            return function;
        }
    }
}