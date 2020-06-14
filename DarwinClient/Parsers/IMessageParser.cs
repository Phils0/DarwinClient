using Apache.NMS;

namespace DarwinClient.Parsers
{
    public interface IMessageParser
    {
        bool TryParse(IMessage source, out Message parsed);
    }
}