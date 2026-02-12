using DarwinClient.Schema.TimetableReference;

namespace DarwinClient
{
    public record TimetableReferenceFile(string Name, int Version, PportTimetableRef? Data)
    {
        public override string ToString() => Name;
    }
}