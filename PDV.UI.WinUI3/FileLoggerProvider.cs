using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace PDV.UI.WinUI3
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;

        public FileLoggerProvider(string logDirectory)
        {
            _logDirectory = logDirectory;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_logDirectory);
        }

        public void Dispose() { }

        private class FileLogger : ILogger
        {
            private readonly string _logDirectory;
            private static readonly object _lock = new object();

            public FileLogger(string logDirectory)
            {
                _logDirectory = logDirectory;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                var logFile = Path.Combine(_logDirectory, $"pdv-log-{DateTime.Now:yyyy-MM-dd}.txt");
                var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {formatter(state, exception)}";

                if (exception != null)
                    message += Environment.NewLine + exception.ToString();

                lock (_lock)
                {
                    File.AppendAllText(logFile, message + Environment.NewLine);
                }
            }
        }
    }
}
