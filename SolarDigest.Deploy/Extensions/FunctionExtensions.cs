using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;

namespace SolarDigest.Deploy.Extensions
{
    public static class FunctionExtensions
    {
        public static IFunction AddPolicyStatements(this IFunction function, params PolicyStatement[] statements)
        {
            foreach (var statement in statements)
            {
                function.AddToRolePolicy(statement);
            }

            return function;
        }

        public static IFunction GrantReadTableData(this IFunction function, params ITable[] tables)
        {
            foreach (var table in tables)
            {
                table.GrantReadData(function);
            }

            return function;
        }

        public static IFunction GrantWriteTableData(this IFunction function, params ITable[] tables)
        {
            foreach (var table in tables)
            {
                table.GrantWriteData(function);
            }

            return function;
        }

        public static IFunction GrantReadWriteTableData(this IFunction function, params ITable[] tables)
        {
            foreach (var table in tables)
            {
                table.GrantReadWriteData(function);
            }

            return function;
        }

        public static IFunction GrantStreamReadData(this IFunction function, params ITable[] tables)
        {
            foreach (var table in tables)
            {
                table.GrantStreamRead(function);
            }

            return function;
        }

        public static IFunction AddEventSource(this IFunction function, params ITable[] tables)
        {
            foreach (var table in tables)
            {
                table.AddEventSource(function);
            }

            return function;
        }
    }
}