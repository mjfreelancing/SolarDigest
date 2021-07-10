using System;

namespace SolarDigest.Api.Logging
{
    public interface ISolarDigestLogger
    {
        void LogDebug(string message);
        void LogException(Exception exception);
    }
}