using System;
using System.IO;
using DarwinClient.SchemaV16;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class ArchiveTest
    {
        public static string ReferenceData = Path.Combine(".", "Data", "20200417020641_ref_v3.xml.gz");
        public static string Timetable = Path.Combine(".", "Data", "20200417020641_v8.xml.gz");
        
        [Fact]
        public void Extract()
        {
            var archive = new ReferenceArchive(ReferenceData, Substitute.For<ILogger>());

            using (var reader = archive.Extract())
            {
                Assert.False( string.IsNullOrEmpty(reader.ReadLine()));
            }
        }

        [Fact]
        public void DeserializeReference()
        {
            var archive = new ReferenceArchive(ReferenceData, Substitute.For<ILogger>());
            var refData = archive.Deserialize();
            Assert.NotNull(refData);
        }

        [Fact]
        public void DeserilizeTimetable()
        {
            var archive = new TimetableArchive(Timetable, Substitute.For<ILogger>());
            var timetable = archive.Deserialize();
            Assert.NotNull(timetable);
        }
    }
}