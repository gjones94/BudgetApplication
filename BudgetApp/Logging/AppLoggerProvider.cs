using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace BudgetApp.Logging
{
    public class AppLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, AppLogger> _loggers = new (StringComparer.OrdinalIgnoreCase);
        private AppLoggerConfiguration _configuration;

        public AppLoggerProvider(AppLoggerConfiguration config)
        {
            _configuration = config;
        }

        public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, new AppLogger(categoryName, GetCurrentConfig));

        private AppLoggerConfiguration GetCurrentConfig() => _configuration; 

        public void Dispose()
        {
            return;
        }
    }
}
