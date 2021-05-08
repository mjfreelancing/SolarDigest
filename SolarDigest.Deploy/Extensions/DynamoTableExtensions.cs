using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;

namespace SolarDigest.Deploy.Extensions
{
    public static class DynamoTableExtensions
    {
        public static void GrantReadDataToFunctions(this ITable table, params IFunction[] functions)
        {
            foreach (var function in functions)
            {
                table.GrantReadData(function);
            }
        }

        public static void GrantWriteDataToFunctions(this ITable table, params IFunction[] functions)
        {
            foreach (var function in functions)
            {
                table.GrantWriteData(function);
            }
        }

        public static void GrantReadWriteDataToFunctions(this ITable table, params IFunction[] functions)
        {
            foreach (var function in functions)
            {
                table.GrantReadData(function);
                table.GrantWriteData(function);
            }
        }

        public static void AddEventSource(this ITable table, IFunction function)
        {
            var exceptionTableEventSource = new DynamoEventSource(table, new DynamoEventSourceProps
            {
                StartingPosition = StartingPosition.LATEST,
                RetryAttempts = 0
            });

            function.AddEventSource(exceptionTableEventSource);
        }
    }
}