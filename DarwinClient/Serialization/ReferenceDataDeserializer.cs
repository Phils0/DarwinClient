using DarwinClient.Schema.TimetableReference;
using Serilog;

namespace DarwinClient.Serialization
{
    public class ReferenceDataDeserializer(ILogger logger) : Deserializer<PportTimetableRef>(logger);
}