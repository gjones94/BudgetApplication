using System.Security.Policy;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BudgetApp.Logging
{
    public class AppLogger : ILogger
    {
        private readonly string _loggingEntity;
        private readonly Func<AppLoggerConfiguration> _getCurrentConfiguration;

        /// <summary>
        /// Custom AppLogger that allows for color customization.
        /// (AppLogger takes in a delegate function that will return the current log color configuration)
        /// </summary>
        /// <param name="loggingEntity"></param>
        /// <param name="getCurrentConfiguration"></param>
        public AppLogger(string loggingEntity, Func<AppLoggerConfiguration> getCurrentConfiguration)
        {
            _loggingEntity = loggingEntity;
            _getCurrentConfiguration = getCurrentConfiguration;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        /// <summary>
        /// Checks whether the given log level has been defined in the application settings
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _getCurrentConfiguration().LogLevelToColor.ContainsKey(logLevel);
        }

        /// <summary>
        /// Custom Log writing method for messages
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if(!IsEnabled(logLevel))
            {
                return;
            }

            AppLoggerConfiguration logConfig = _getCurrentConfiguration();

            //save previous color of console
            ConsoleColor originalColor = Console.ForegroundColor;

            //2 and -12 are padding values
            Console.ForegroundColor = logConfig.LogLevelToColor[logLevel];
            Console.WriteLine($"[{eventId, 2}: {logLevel, -12}]");

            Console.ForegroundColor = logConfig.LoggingEntityColor;
            Console.Write($"     {_loggingEntity} - ");

            Console.ForegroundColor = logConfig.LogLevelToColor[logLevel];
            Console.Write($"{formatter(state, exception)}");

            //reset to whatever the previous color was
            Console.ForegroundColor = originalColor;
            Console.WriteLine(Environment.NewLine);
        }
    }
}
