namespace BudgetApp.Logging
{
    public sealed class AppLoggerConfiguration
    {
        public Dictionary<LogLevel, ConsoleColor> LogLevelToColor { get; set; } = new Dictionary<LogLevel, ConsoleColor>()
        {
            { LogLevel.Information, ConsoleColor.Cyan },
            { LogLevel.Warning, ConsoleColor.Yellow },
            { LogLevel.Debug, ConsoleColor.Blue },
            { LogLevel.Error , ConsoleColor.Magenta },
            { LogLevel.Critical, ConsoleColor.Red }
        };

        public ConsoleColor LoggingEntityColor = ConsoleColor.DarkCyan;

    }
}
