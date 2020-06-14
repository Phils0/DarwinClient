using Serilog;
using Serilog.Formatting.Display;
using Xunit.Abstractions;

namespace DarwinClient.Test.Helpers
{
    internal static class LoggingHelper
    {
        public static ILogger CreateLogger(ITestOutputHelper testOutputHelper)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(testOutputHelper)
                .CreateLogger();
        }
        
        public static ILogger CreateMessageLogger(ITestOutputHelper testOutputHelper)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.TestOutput(testOutputHelper, new MessageTemplateTextFormatter("{Message}"))
                .CreateLogger();
        }
    }
}