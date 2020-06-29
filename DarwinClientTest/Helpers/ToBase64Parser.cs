using System;
using Apache.NMS;
using DarwinClient.Parsers;
using Serilog;

namespace DarwinClient.Test.Helpers
{
    /// <summary>
    /// Used to help generate test messages
    /// <see cref="MessageGenerator"/> for generating test messages
    /// </summary>
    public class ToBase64Parser : IMessageParser
    {
        public Type MessageType { get; } = typeof(TextMessage);

        private readonly ILogger _logger;

        public ToBase64Parser(ILogger logger)
        {
            _logger = logger;
        }
        public bool TryParse(IMessage source, out Message parsed)
        {
            string msg;
            if (source is IBytesMessage byteMessage)
            {
                msg = Convert.ToBase64String(byteMessage.Content);
            }
            else
            {
                parsed = null;
                return false;
            }
            
            _logger.Debug("{timestamp}:{msg}: {text}", source.NMSTimestamp, source.NMSMessageId, msg);
            parsed = new TextMessage(msg, source);
            return true;
        }
    }
}