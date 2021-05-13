﻿using System;

namespace SolarDigest.Api.Logging
{
    public interface IFunctionLogger
    {
        void LogDebug(string message);
        void LogException(Exception exception);
    }
}