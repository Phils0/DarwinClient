using System;
using System.Diagnostics.CodeAnalysis;
using Apache.NMS;

namespace DarwinClient.Parsers
{
    public interface IMessageParser
    {
        Type MessageType { get; }
        bool TryParse(IMessage source, [MaybeNullWhen(false)] out Message parsed);
    }
}