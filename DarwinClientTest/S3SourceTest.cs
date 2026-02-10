using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient.Test.Helpers;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace DarwinClient.Test
{
    public class S3SourceTest
    {
        private readonly ILogger _logger;

        public S3SourceTest(ITestOutputHelper testOutputHelper)
        {
            _logger = LoggingHelper.CreateLogger(testOutputHelper);
        }
        
        [Fact(Skip = "Actual S3 calls, needs profile set")]
        public async Task RealS3GetLatest()
        {
            var source = new S3Source(Helpers.Amazon.GetS3Client(), _logger);
            var (stream, name) =  await source.Read($"\\d+_ref_v.", CancellationToken.None);

            Assert.NotEmpty(name);
            using var reader = new StreamReader(stream);
            var data = reader.ReadToEnd();
            Assert.NotEmpty(data);        
        }
        
        [Theory]
        [InlineData("20200429\\d+_ref_v2", @"PPTimetable/20200429020643_ref_v2.xml.gz")]
        [InlineData("20200429\\d+_ref_v3", @"PPTimetable/20200429020643_ref_v3.xml.gz")]
        [InlineData("\\d+_ref_v3", @"PPTimetable/20200501020639_ref_v3.xml.gz")]
        [InlineData("20200430\\d+_v8", @"PPTimetable/20200430020644_v8.xml.gz")]
        [InlineData("20200501\\d+_v8", @"PPTimetable/20200501020639_v8.xml.gz")]
        [InlineData("\\d+_v8", @"PPTimetable/20200501020639_v8.xml.gz")]
        public async Task GetLatest(string searchPattern, string expectedFile)
        {
            var source = new S3Source(DummyS3.CreateMock(), _logger);
            var (stream, name) =  await source.Read(searchPattern, CancellationToken.None);
            Assert.Equal(expectedFile, name);
        }
        
        [Fact]
        public async Task GetLatestThrowsExceptionWhenNotFound()
        {
            var source = new S3Source(DummyS3.CreateMock(), _logger);
            var ex = await Assert.ThrowsAsync<DarwinException>(() =>  source.Read("NOTHING", CancellationToken.None));
            Assert.Equal("Failed to download Darwin file NOTHING", ex.Message);
        }
    }
}
