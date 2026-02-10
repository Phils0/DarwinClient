using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using DarwinClient.Test.Helpers;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class TimetableDownloaderTest
    {
        private readonly ILogger _logger;

        public TimetableDownloaderTest(ITestOutputHelper testOutputHelper)
        {
            _logger = LoggingHelper.CreateLogger();
        }

        [Fact(Skip = "Actual S3 calls, needs profile set")]
        public async Task RealDownloadSpecificReferenceData()
        {
             var downloader = CreateDownloader(Helpers.Amazon.GetS3Client());
             var refData =  await downloader.GetReference(DateTime.Today, CancellationToken.None);
            
            Assert.NotNull(refData);
        }

        private TimetableDownloader CreateDownloader(IAmazonS3 client)
        {
            var source = new S3Source(client, _logger);
            return CreateDownloader(source);
        }
        
        private TimetableDownloader CreateDownloader(IDarwinDownloadSource source)
        {
            return new TimetableDownloader(source, _logger);
        }

        [Fact]
        public async Task DownloadSpecificReferenceData()
        {
            var downloader = CreateDownloader(DummyS3.CreateMock());
            var refData = await downloader.GetReference(DummyS3.TestDate, CancellationToken.None);
            
            Assert.NotNull(refData);
            Assert.Equal("PPTimetable/20200429020643_ref_v3.xml.gz", refData.File);
        }
        
        [Fact]
        public async Task DownloadLatestReferenceData()
        {
            var downloader = CreateDownloader(DummyS3.CreateSuccessfulRefDataMock());
            var refData = await downloader.GetLatestReference(CancellationToken.None);
            
            Assert.NotNull(refData);
            Assert.Equal("PPTimetable/20200501020639_ref_v3.xml.gz", refData.File);
        }
        
        [Fact(Skip = "Actual S3 calls, needs profile set")]
        public async Task RealDownloadSpecificTimetable()
        {
            var downloader = CreateDownloader(Helpers.Amazon.GetS3Client());
            var timetable =  await downloader.GetTimetable(DateTime.Today, CancellationToken.None);
            
            Assert.NotNull(timetable);
         }
        
        [Fact]
        public async Task DownloadSpecificTimetable()
        {
            var downloader = CreateDownloader(DummyS3.CreateMock());
            var timetable = await downloader.GetTimetable(DummyS3.TestDate, CancellationToken.None);
            
            Assert.NotNull(timetable);
            Assert.Equal("PPTimetable/20200429020643_v8.xml.gz", timetable.File);
        }
        
        [Fact]
        public async Task DownloadLatestTimetable()
        {
            var downloader = CreateDownloader(DummyS3.CreateSuccessfulTimetableMock());
            var timetable = await downloader.GetLatestTimetable(CancellationToken.None);
            
            Assert.NotNull(timetable);
            Assert.Equal("PPTimetable/20200501020639_v8.xml.gz", timetable.File);
        }
    }
}
