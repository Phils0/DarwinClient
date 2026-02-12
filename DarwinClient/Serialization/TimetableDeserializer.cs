using DarwinClient.Schema.Timetable;
using Serilog;

namespace DarwinClient.Serialization
{
    public class TimetableDeserializer(ILogger logger) : Deserializer<PportTimetable>(logger);
}