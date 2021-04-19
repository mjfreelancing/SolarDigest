using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarDigest.Api.Logging;
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
        public IServiceProvider Services => _services.Value;
        public IConfiguration Configuration => Services.GetService<IConfiguration>();

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

                //var serviceCollection = new ServiceCollection();

                //ConfigureServices(serviceCollection);

                //return serviceCollection.BuildServiceProvider();
            });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IFunctionLogger, FunctionLogger>();
            services.AddScoped<IExceptionHandler, FunctionExceptionHandler>();
        }

        protected abstract Task<TResultType> InvokeHandlerAsync(FunctionContext<TPayload> context);

        // This is the function called by AWS services
        public async Task<TResultType> InvokeAsync(TPayload payload, ILambdaContext context)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedServiceProvider = scope.ServiceProvider;

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
        }
    }
}