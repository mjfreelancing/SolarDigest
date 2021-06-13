using AllOverIt.Helpers;
using SolarDigest.Api.Logging;
using System;

namespace SolarDigest.Api
{
    public sealed class FunctionContext<TPayload>
    {
        public IServiceProvider ScopedServiceProvider { get; }
        public ISolarDigestLogger Logger { get; }
        public TPayload Payload { get; }

        public FunctionContext(IServiceProvider scopedServiceProvider, ISolarDigestLogger logger, TPayload payload)
        {
            ScopedServiceProvider = scopedServiceProvider.WhenNotNull(nameof(scopedServiceProvider));
            Logger = logger.WhenNotNull(nameof(logger));
            Payload = payload;
        }
    }
}