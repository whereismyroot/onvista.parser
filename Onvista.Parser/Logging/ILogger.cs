using System;

namespace Onvista.Parser.Logging
{
    public interface ILogger
    {
        void LogError(string message, Exception exception = null, params object[] propertyValues);

        void LogInformation(string message);

        void LogWarning(string message);
    }
}
