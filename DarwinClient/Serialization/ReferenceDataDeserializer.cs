using DarwinClient.Schema;
using Serilog;

namespace DarwinClient.Serialization
{
    public class ReferenceDataDeserializer(ILogger logger) : Deserializer<PportTimetableRef>(logger);
}