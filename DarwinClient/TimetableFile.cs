using DarwinClient.Schema;

namespace DarwinClient
{
    public record TimetableFile(string Name, PportTimetable? Data)
    {
        public override string ToString() => Name;
    }
}