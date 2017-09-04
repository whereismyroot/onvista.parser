using System;
using System.IO;
using Serilog;

namespace Onvista.Parser.Logging
{
    public class ParserLogger : ILogger
    {
        private readonly Serilog.Core.Logger _logger;

        public ParserLogger()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "logs", "log-{Date}.txt"))
                .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                .CreateLogger();
        }

        public void LogError(string message, Exception exception = null, params object[] propertyValues)
        {
            if (exception != null)
            {
                _logger.Error(exception, message, propertyValues);
            }
            else
            {
                _logger.Error(message);
            }
        }

        public void LogInformation(string message)
        {

            _logger.Information(message);

        }

        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }
    }
}
