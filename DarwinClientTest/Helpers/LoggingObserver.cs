using System;
using Serilog;

namespace DarwinClient.Test.Helpers
{
    internal class LoggingObserver : IPushPortObserver
    {
        public Type MessageType { get; } = typeof(TextMessage);
        
        private readonly ILogger _logger;

        public LoggingObserver(ILogger logger)
        {
            _logger = logger;
        }
        
        public void OnCompleted()
        {
            _logger.Information("PushPort Completed");
        }

        public void OnError(Exception error)
        {
            _logger.Error(error, "PushPort Error.");
        }

        public void OnNext(Message msg)
        {
            if(msg is UnknownMessage)
                _logger.Warning("{message}", msg);
            else
                _logger.Information("{message}", msg);
        }

        public void Dispose()
        {
            _logger.Information("Dispose");
        }
    }
}