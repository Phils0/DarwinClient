using System;
using System.Threading;
using NSubstitute;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace DarwinClient.Test
{
    public class S3TimetableDownloaderTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger _logger;

        public S3TimetableDownloaderTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(testOutputHelper)
                .CreateLogger();
        }

        [Fact(Skip = "Actual S3 calls, needs profile set")]
        public async void RealDownloadSpecificReferenceData()
        {
            var client = Amazon.GetS3Client();
            var downloader = new S3TimetableDownloader(client, _logger);

            var refData =  await downloader.GetReference(DateTime.Today, CancellationToken.None);
            
            Assert.NotNull(refData);
        }
        
        [Fact]
        public async void DownloadSpecificReferenceData()
        {
            var client = DummyS3.CreateMock();
            var downloader = new S3TimetableDownloader(client, _logger);

            var refData = await downloader.GetReference(DummyS3.TestDate, CancellationToken.None);
            
            Assert.NotNull(refData);
            Assert.Equal("PPTimetable/20200429020643_ref_v3.xml.gz", refData.File);
        }
        
        [Fact]
        public async void DownloadLatestReferenceData()
        {
            var client = DummyS3.CreateSuccessfulRefDataMock();
            var downloader = new S3TimetableDownloader(client, _logger);

            var refData = await downloader.GetLatestReference(CancellationToken.None);
            
            Assert.NotNull(refData);
            Assert.Equal("PPTimetable/20200501020639_ref_v3.xml.gz", refData.File);
        }
        
        [Fact(Skip = "Actual S3 calls, needs profile set")]
        public async void RealDownloadSpecificTimetable()
        {
            var client = Amazon.GetS3Client();
            var downloader = new S3TimetableDownloader(client, _logger);

            var timetable =  await downloader.GetTimetable(DateTime.Today, CancellationToken.None);
            
            Assert.NotNull(timetable);
         }
        
        [Fact]
        public async void DownloadSpecificTimetable()
        {
            var client = DummyS3.CreateMock();
            var downloader = new S3TimetableDownloader(client, _logger);

            var timetable = await downloader.GetTimetable(DummyS3.TestDate, CancellationToken.None);
            
            Assert.NotNull(timetable);
            Assert.Equal("PPTimetable/20200429020643_v8.xml.gz", timetable.File);
        }
        
        [Fact]
        public async void DownloadLatestTimetable()
        {
            var client = DummyS3.CreateSuccessfulTimetableMock();
            var downloader = new S3TimetableDownloader(client, _logger);

            var timetable = await downloader.GetLatestTimetable(CancellationToken.None);
            
            Assert.NotNull(timetable);
            Assert.Equal("PPTimetable/20200501020639_v8.xml.gz", timetable.File);
        }
    }
}