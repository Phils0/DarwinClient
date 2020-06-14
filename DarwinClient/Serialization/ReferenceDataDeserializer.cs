using DarwinClient.SchemaV16;
using Serilog;

namespace DarwinClient.Serialization
{
    public class ReferenceDataDeserializer : Deserializer<PportTimetableRef>
    {
        public ReferenceDataDeserializer(ILogger logger) :
            base(logger)
        {
        }
    }
}