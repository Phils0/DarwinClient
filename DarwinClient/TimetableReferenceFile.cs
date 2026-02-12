using DarwinClient.Schema.TimetableReference;

namespace DarwinClient
{
    public record TimetableReferenceFile(string Name, PportTimetableRef? Data)
    {
        public override string ToString() => Name;
    }
}