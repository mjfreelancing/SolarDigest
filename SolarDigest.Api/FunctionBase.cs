using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Mapping;
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
        protected IConfiguration Configuration => Services.GetService<IConfiguration>();

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
            services.AddScoped<ISolarEdgeApi, SolarEdgeApi>();
            services.AddAutoMapper(typeof(SolarViewProfile));

            // for troubleshooting mapping
            //var provider = services.AddAutoMapper(typeof(SolarViewProfile)).BuildServiceProvider();
            //provider.GetService<IMapper>()!.ConfigurationProvider.AssertConfigurationIsValid();
        }

        protected abstract Task<TResultType> InvokeHandlerAsync(FunctionContext<TPayload> context);

        // This is the function called by AWS services
        public async Task<TResultType> InvokeAsync(TPayload payload, ILambdaContext context)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedServiceProvider = scope.ServiceProvider;

                try
                {
                    var logger = scopedServiceProvider.GetService<IFunctionLogger>();

                    if (logger is FunctionLogger functionLogger)
                    {
                        // the logger is context specific so needs to be injected manually
                        functionLogger.SetLambdaLogger(context.Logger);
                    }

                    logger!.LogDebug($"Invoked: {context.FunctionName}");

                    var handlerContext = new FunctionContext<TPayload>(scopedServiceProvider, logger, payload);
                    return await InvokeHandlerAsync(handlerContext).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    var exceptionHandler = scopedServiceProvider.GetService<IExceptionHandler>();
                    await exceptionHandler!.HandleAsync(exception);
                }

                // todo: Possible exceptions include
                //       SolarEdgeResponseException

                // todo: really should wrap the result in a response object with a success / fail status ?
                return default;
            }
        }
    }
}