using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Apache.NMS;
using DarwinClient.Schema;
using DarwinClient.Serialization;
using Serilog;

namespace DarwinClient.Parsers
{
    public class ToDarwinMessageParser : IMessageParser
    {
        public Type MessageType { get; } = typeof(DarwinMessage);
        
        private readonly ILogger _logger;
        private readonly MessageDeserializer _deserializer;

        public ToDarwinMessageParser(ILogger logger)
        {
            _logger = logger;
            _deserializer = new MessageDeserializer(logger);
        }
        
        public bool TryParse(IMessage source, [MaybeNullWhen(false)] out Message parsed)
        {
            Pport? darwinMsgs = null;
            
            if (source is IBytesMessage byteMessage)
            {
                _logger.Debug("{timestamp}:{msg}", source.NMSTimestamp, source.NMSMessageId);
                using var stream = new MemoryStream(byteMessage.Content); 
                darwinMsgs = _deserializer.Deserialize(stream, source.NMSMessageId);
            }
            else if (source is ITextMessage textMessage)
            {
                _logger.Debug("{timestamp}:{msg}: {text}", source.NMSTimestamp, source.NMSMessageId, textMessage.Text);
                darwinMsgs = _deserializer.Deserialize(textMessage.Text, source.NMSMessageId);
            }
            else
            {
                parsed = null;
                return false;
            }
            
            if(darwinMsgs == null)
            {
                _logger.Error("DarwinMessage not set {timestamp}:{msg}", source.NMSTimestamp, source.NMSMessageId);
                parsed = null;
                return false;
            }
            
            parsed = new DarwinMessage(darwinMsgs, source);
            return true;
        }
    }
}