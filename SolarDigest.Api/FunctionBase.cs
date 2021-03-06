﻿using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarDigest.Api.Aggregators;
using SolarDigest.Api.Exceptions;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Validators;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Mapping;
using SolarDigest.Api.Repository;
using SolarDigest.Api.Services;
using SolarDigest.Api.Services.SolarEdge;
using SolarDigest.Api.Summarizers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// .Net Core 3.1 has improved performance using [DefaultLambdaJsonSerializer] or [CamelCaseLambdaJsonSerializer]
// The two different versions exists due to issues with the first release of [Amazon.Lambda.Serialization.SystemTextJson]
// as described here: https://aws.amazon.com/blogs/developer/one-month-update-to-net-core-3-1-lambda/
[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace SolarDigest.Api
{
    public abstract class FunctionBase<TPayload, TResultType>
    {
        // Refers to exception types not reported via email
        private readonly IList<Type> _exceptionTypesNotReported = new List<Type>(new[]
        {
            typeof(SolarDigestValidationException)
        });

        private readonly Lazy<IServiceProvider> _services;
        private IServiceProvider Services => _services.Value;

        // This can be shared across scopes since it is registered as a Singleton
        protected IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

        protected FunctionBase()
        {
            _services = new Lazy<IServiceProvider>(() =>
            {
                var builder = Host
                    .CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton(hostContext.Configuration);
                        ConfigureServices(services);
                    });

                return builder.Build().Services;
            });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISolarDigestLogger, SolarDigestLogger>();
            services.AddScoped<IExceptionHandler, PersistExceptionHandler>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<ISiteUpdater, SiteUpdater>();
            services.AddScoped<IParameterStore, ParameterStore>();
            services.AddScoped<IPresignedUrlCreator, PresignedUrlCreator>();
            services.AddScoped<GetSitePayloadValidator>();
            services.AddScoped<AddSitePayloadValidator>();
            services.AddScoped<UpdateSitePayloadValidator>();
            services.AddScoped<GetSitePowerSummaryPayloadValidator>();
            services.AddScoped<AggregateSitePowerPayloadValidator>();
            services.AddScoped<IDailyAveragePowerSummarizer, DailyAveragePowerSummarizer>();
            services.AddScoped<ISolarDigestSiteTable, SolarDigestSiteTable>();
            services.AddScoped<ISolarDigestExceptionTable, SolarDigestExceptionTable>();
            services.AddScoped<ISolarDigestPowerTable, SolarDigestPowerTable>();
            services.AddScoped<ISolarDigestPowerMonthlyTable, SolarDigestPowerMonthlyTable>();
            services.AddScoped<ISolarDigestPowerYearlyTable, SolarDigestPowerYearlyTable>();
            services.AddScoped<ISolarDigestPowerUpdateHistoryTable, SolarDigestPowerUpdateHistoryTable>();
            services.AddScoped<IPowerMonthlyAggregator, PowerMonthlyAggregator>();
            services.AddScoped<IPowerYearlyAggregator, PowerYearlyAggregator>();
            services.AddScoped<ISolarEdgeApi, SolarEdgeApi>();
            services.AddAutoMapper(typeof(SolarViewProfile));

            // for troubleshooting mapping
            //var provider = services.AddAutoMapper(typeof(SolarViewProfile)).BuildServiceProvider();
            //provider.GetRequiredService<IMapper>()!.ConfigurationProvider.AssertConfigurationIsValid();
        }

        protected abstract Task<TResultType> InvokeHandlerAsync(FunctionContext<TPayload> context);

        // This is the function called by AWS services
        public async Task<LambdaResult<TResultType>> InvokeAsync(TPayload payload, ILambdaContext context)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedServiceProvider = scope.ServiceProvider;

                var logger = scopedServiceProvider.GetRequiredService<ISolarDigestLogger>();

                if (logger is SolarDigestLogger functionLogger)
                {
                    // the logger is context specific so needs to be injected manually
                    functionLogger.SetLambdaLogger(context.Logger);
                }

                logger!.LogDebug($"Invoked: {context.FunctionName}");

                try
                {
                    if (payload is IRequiresNormalisation model)
                    {
                        // For example, convert FEED_IN to FEEDIN so it can later be interpreted as the enum MeterType.FeedIn
                        model.Normalise();
                    }

                    var handlerContext = new FunctionContext<TPayload>(scopedServiceProvider, logger, payload);
                    var result = await InvokeHandlerAsync(handlerContext).ConfigureAwait(false);

                    return new LambdaResult<TResultType>(result);
                }
                catch (Exception exception)
                {
                    if (!_exceptionTypesNotReported.Contains(exception.GetType()))
                    {
                        // known types include: SolarEdgeResponseException, SolarEdgeTimeoutException, DynamoDbConflictException
                        await ReportException(scopedServiceProvider, exception, logger);
                    }

                    return new LambdaResult<TResultType>(exception);
                }
            }
        }

        private static Task ReportException(IServiceProvider serviceProvider, Exception exception, ISolarDigestLogger logger)
        {
            logger.LogException(exception);

            var exceptionHandler = serviceProvider.GetRequiredService<IExceptionHandler>();
            return exceptionHandler!.HandleAsync(exception);
        }
    }
}