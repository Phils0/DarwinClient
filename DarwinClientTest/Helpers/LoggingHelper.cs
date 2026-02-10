using Serilog;
using Serilog.Formatting.Display;
using Serilog.Sinks.XUnit3;
using Xunit;

namespace DarwinClient.Test.Helpers
{
    internal static class LoggingHelper
    {
        public static ILogger CreateLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.XUnit3TestOutput()
                .CreateLogger();
        }
    }
}