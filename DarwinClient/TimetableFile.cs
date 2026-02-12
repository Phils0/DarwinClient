using DarwinClient.Schema.Timetable;

namespace DarwinClient
{
    public record TimetableFile(string Name, int Version, PportTimetable? Data)
    {
        public override string ToString() => Name;
    }
}