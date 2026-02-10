using System;
using Apache.NMS;

namespace DarwinClient.Parsers
{
    public interface IMessageParser
    {
        Type MessageType { get; }
        bool TryParse(IMessage source, out Message? parsed);
    }
}