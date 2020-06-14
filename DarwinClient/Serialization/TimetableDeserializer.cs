using DarwinClient.SchemaV16;
using Serilog;

namespace DarwinClient.Serialization
{
    public class TimetableDeserializer : Deserializer<PportTimetable>
    {
        public TimetableDeserializer(ILogger logger) :
            base(logger)
        {
        }
    }
}