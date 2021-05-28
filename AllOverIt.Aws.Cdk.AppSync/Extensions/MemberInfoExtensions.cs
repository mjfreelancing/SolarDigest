using System.Linq;
using System.Reflection;
using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Factories;
using Amazon.CDK.AWS.AppSync;

namespace AllOverIt.Aws.Cdk.AppSync.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static BaseDataSource GetMethodDataSource(this MemberInfo methodInfo, IDataSourceFactory dataSourceFactory)
        {
            var attribute = methodInfo.GetCustomAttributes(typeof(DataSourceAttribute), true).SingleOrDefault();

            return attribute == null
                ? null
                : dataSourceFactory.CreateDataSource(attribute as DataSourceAttribute);
        }

        public static string GetFunctionName(this MemberInfo methodInfo)
        {
            var attribute = methodInfo.GetCustomAttributes(typeof(DataSourceAttribute), true).SingleOrDefault();

            if (attribute is LambdaDataSourceAttribute lambdaDataSourceAttribute)
            {
                return lambdaDataSourceAttribute.FunctionName;
            }

            return null;
        }
    }
}