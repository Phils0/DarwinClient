using DarwinClient.Schema.Timetable;

namespace DarwinClient
{
    public record TimetableFile(string Name, PportTimetable? Data)
    {
        public override string ToString() => Name;
    }
}