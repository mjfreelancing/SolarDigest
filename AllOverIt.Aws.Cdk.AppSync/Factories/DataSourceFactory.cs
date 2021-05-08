﻿using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Helpers;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.Lambda;
using System.Collections.Generic;

namespace AllOverIt.Aws.Cdk.AppSync.Factories
{
    public sealed class DataSourceFactory : IDataSourceFactory
    {
        private readonly IDictionary<string, BaseDataSource> _dataSourceCache = new Dictionary<string, BaseDataSource>();

        private readonly IGraphqlApi _graphQlApi;
        //private readonly IDataSourceRoleCache _serviceRoles;

        public DataSourceFactory(IGraphqlApi graphQlApi/*, IDataSourceRoleCache serviceRoles*/)
        {
            _graphQlApi = graphQlApi.WhenNotNull(nameof(graphQlApi));
            //_serviceRoles = serviceRoles.WhenNotNull(nameof(serviceRoles));
        }

        public BaseDataSource CreateDataSource(DataSourceAttribute attribute)
        {
            if (!_dataSourceCache.TryGetValue(attribute.LookupKey, out var dataSource))
            {
                dataSource = attribute switch
                {
                    LambdaDataSourceAttribute lambdaDataSourceAttribute => CreateLambdaDataSource(lambdaDataSourceAttribute),
                    _ => null
                };

                _dataSourceCache.Add(attribute.LookupKey, dataSource);
            }

            return dataSource;
        }

        private BaseDataSource CreateLambdaDataSource(LambdaDataSourceAttribute attribute)
        {
            var stack = Stack.Of(_graphQlApi);

            return new LambdaDataSource(stack, $"{attribute.LookupKey}DataSource", new LambdaDataSourceProps
            {
                Api = _graphQlApi,
                Name = $"{attribute.LookupKey}DataSource",
                //ServiceRole = _serviceRoles.GetRole(attribute.ServiceName, attribute.FunctionName),

                LambdaFunction = Function.FromFunctionArn(stack, $"{attribute.ServiceName}{attribute.FunctionName}Function",
                    $"arn:aws:lambda:{stack.Region}:{stack.Account}:function:{attribute.ServiceName}_{attribute.FunctionName}")
            });
        }
    }
}