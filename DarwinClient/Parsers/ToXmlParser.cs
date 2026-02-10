using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using Apache.NMS;
using Serilog;

namespace DarwinClient.Parsers
{
    public class ToXmlParser : IMessageParser
    {
        public Type MessageType { get; } = typeof(TextMessage);

        private readonly ILogger _logger;

        public ToXmlParser(ILogger logger)
        {
            _logger = logger;
        }
        
        public bool TryParse(IMessage source, [MaybeNullWhen(false)]  out Message parsed)
        {
            string msg;
            if (source is IBytesMessage byteMessage)
            {
                using var stream = new MemoryStream(byteMessage.Content); 
                using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
                using var reader = new StreamReader(decompressionStream);
                msg = reader.ReadToEnd();
            }
            else if (source is ITextMessage textMessage)
            {
                msg = textMessage.Text;
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