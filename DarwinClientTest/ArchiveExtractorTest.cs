using System;
using System.IO;
using DarwinClient.SchemaV16;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class ArchiveExtractorTest
    {
        
        [Fact]
        public void DeserializeReference()
        {
            var archive = new ReferenceDataExtractor(Substitute.For<ILogger>());
            using var stream = File.OpenRead(DummyS3.ReferenceData);
            var refData = archive.Deserialize(stream, DummyS3.ReferenceData);
            Assert.NotNull(refData);
        }

        [Fact]
        public void DeserializeTimetable()
        {
            var archive = new TimetableExtractor(Substitute.For<ILogger>());
            using var stream = File.OpenRead(DummyS3.Timetable);
            var timetable = archive.Deserialize(stream, DummyS3.Timetable);
            Assert.NotNull(timetable);
        }
    }
}