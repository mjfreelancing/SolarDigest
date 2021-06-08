using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarDigest.Api.Exceptions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Mapping;
using SolarDigest.Api.Processors;
using SolarDigest.Api.Repository;
using SolarDigest.Api.Services;
using SolarDigest.Api.Services.SolarEdge;
using System;
using System.Threading.Tasks;

// .Net Core 3.1 has improved performance using [DefaultLambdaJsonSerializer] or [CamelCaseLambdaJsonSerializer]
// The two different versions exists due to issues with the first release of [Amazon.Lambda.Serialization.SystemTextJson]
// as described here: https://aws.amazon.com/blogs/developer/one-month-update-to-net-core-3-1-lambda/
[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace SolarDigest.Api
{
    public abstract class FunctionBase<TPayload, TResultType>
    {
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
            services.AddScoped<IFunctionLogger, FunctionLogger>();
            services.AddScoped<IExceptionHandler, PersistExceptionHandler>();
            services.AddScoped<ISolarDigestSiteTable, SolarDigestSiteTable>();
            services.AddScoped<ISolarDigestExceptionTable, SolarDigestExceptionTable>();
            services.AddScoped<ISolarDigestPowerTable, SolarDigestPowerTable>();
            services.AddScoped<ISolarDigestPowerMonthlyTable, SolarDigestPowerMonthlyTable>();
            services.AddScoped<ISolarDigestPowerYearlyTable, SolarDigestPowerYearlyTable>();
            services.AddScoped<ISolarDigestPowerUpdateHistoryTable, SolarDigestPowerUpdateHistoryTable>();
            services.AddTransient<IPowerMonthlyProcessor, PowerMonthlyProcessor>();
            services.AddTransient<IPowerYearlyProcessor, PowerYearlyProcessor>();
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

                var logger = scopedServiceProvider.GetRequiredService<IFunctionLogger>();

                if (logger is FunctionLogger functionLogger)
                {
                    // the logger is context specific so needs to be injected manually
                    functionLogger.SetLambdaLogger(context.Logger);
                }

                logger!.LogDebug($"Invoked: {context.FunctionName}");

                try
                {
                    var handlerContext = new FunctionContext<TPayload>(scopedServiceProvider, logger, payload);
                    var result = await InvokeHandlerAsync(handlerContext).ConfigureAwait(false);

                    return new LambdaResult<TResultType>(result);
                }

                // todo: consider creating a registration of exception handlers and consolidate this code

                catch (SolarEdgeResponseException exception)
                {
                    await ReportException(scopedServiceProvider, exception, logger);
                    return new LambdaResult<TResultType>(exception);
                }
                catch (DynamoDbConflictException exception)
                {
                    await ReportException(scopedServiceProvider, exception, logger);
                    return new LambdaResult<TResultType>(exception);
                }
                catch (Exception exception)
                {
                    await ReportException(scopedServiceProvider, exception, logger);
                    return new LambdaResult<TResultType>(exception);
                }
            }
        }

        private static Task ReportException(IServiceProvider serviceProvider, Exception exception, IFunctionLogger logger)
        {
            logger.LogException(exception);

            var exceptionHandler = serviceProvider.GetRequiredService<IExceptionHandler>();
            return exceptionHandler!.HandleAsync(exception);
        }
    }
}