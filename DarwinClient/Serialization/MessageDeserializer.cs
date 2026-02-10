using DarwinClient.Schema;
using Serilog;

namespace DarwinClient.Serialization
{
    public class MessageDeserializer : Deserializer<Pport>
    {
        public MessageDeserializer(ILogger logger) :
            base(logger)
        {
        }
    }
}