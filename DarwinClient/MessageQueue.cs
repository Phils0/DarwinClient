using System;
using System.Collections.Generic;
using DarwinClient.SchemaV16;
using Serilog;

namespace DarwinClient
{
    public class MessageQueue : Queue<Pport>, IObserver<Message>
    {
        private readonly ILogger _logger;

        public MessageQueue(ILogger logger)
        {
            _logger = logger;
        }
        
        public void OnCompleted()
        {
            // Do nothing
        }

        public void OnError(Exception error)
        {
            // Do nothing
        }

        public void OnNext(Message msg)
        {
            if(msg is DarwinMessage darwinMessage)
                Enqueue(darwinMessage.Updates);
            else
                _logger.Warning("Unknown message: {msg}", msg);
        }
    }
}