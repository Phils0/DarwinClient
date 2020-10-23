using System;
using System.Threading;
using Amazon.S3;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace DarwinClient.Test
{
    public class TimetableDownloaderFileSourceTest
    {
        private readonly ILogger _logger;

        public TimetableDownloaderFileSourceTest(ITestOutputHelper testOutputHelper)
        {
            _logger = LoggingHelper.CreateLogger(testOutputHelper);
        }
        
        private TimetableDownloader CreateDownloader()
        {
            var source = new FileSource(DummyS3.Directory, _logger);
            return new TimetableDownloader(source, _logger);
        }

        [Fact]
        public async void DownloadSpecificReferenceData()
        {
            var downloader = CreateDownloader();
            var refData = await downloader.GetReference(DummyS3.TestDate, CancellationToken.None);
            
            Assert.NotNull(refData);
            Assert.Equal("20200429020643_ref_v3.xml.gz", refData.File);
        }
        
        [Fact]
        public async void DownloadLatestReferenceData()
        {
            var downloader = CreateDownloader();
            var refData = await downloader.GetLatestReference(CancellationToken.None);
            
            Assert.NotNull(refData);
            Assert.Equal("20200429020643_ref_v3.xml.gz", refData.File);
        }
        
        [Fact]
        public async void DownloadSpecificTimetable()
        {
            var downloader = CreateDownloader();
            var timetable = await downloader.GetTimetable(DummyS3.TestDate, CancellationToken.None);
            
            Assert.NotNull(timetable);
            Assert.Equal("20200429020643_v8.xml.gz", timetable.File);
        }
        
        [Fact]
        public async void DownloadLatestTimetable()
        {
            var downloader = CreateDownloader();
            var timetable = await downloader.GetLatestTimetable(CancellationToken.None);
            
            Assert.NotNull(timetable);
            Assert.Equal("20200429020643_v8.xml.gz", timetable.File);
        }
    }
}