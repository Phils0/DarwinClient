using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient.Test.Helpers;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class FileSourceTest
    {
        private readonly ILogger _logger = LoggingHelper.CreateLogger();

        private DirectoryInfo Directory => DummyS3.Directory;

        [Fact] 
        public async Task RealFileGetLatest()
        {
            var source = new FileSource(Directory, _logger);
            var (stream, name) =  await source.Read($"\\d+_ref_v.", CancellationToken.None);

            Assert.NotEmpty(name);
            using var reader = new StreamReader(stream);
            var data = reader.ReadToEnd();
            Assert.NotEmpty(data);        
        }
        
        [Theory]
        [InlineData("20260201\\d+_ref_v4", @"20260201020500_ref_v4.xml.gz")]
        [InlineData("20200429\\d+_ref_v3", @"20200429020643_ref_v3.xml.gz")]
        [InlineData("\\d+_ref_v3", @"20260201020500_ref_v3.xml.gz")]
        [InlineData("\\d+_ref_v4", @"20260201020500_ref_v4.xml.gz")]
        [InlineData("20260201\\d+_v8", @"20260201020500_v8.xml.gz")]
        [InlineData("\\d+_v8", @"20260201020500_v8.xml.gz")]
        public async Task ReadReturnLatestFileThatSatisfiesTheSearch(string searchPattern, string expectedFile)
        {
            var source = new FileSource(Directory, _logger);
            var (stream, name) =  await source.Read(searchPattern, CancellationToken.None);
            Assert.Equal(expectedFile, name);
        }
        
        [Fact]
        public async Task ReadThrowsExceptionWhenNotFound()
        {
            var source = new FileSource(Directory, _logger);
            var ex = await Assert.ThrowsAsync<DarwinException>(() =>  source.Read("NOTHING", CancellationToken.None));
            Assert.Equal("Failed to download Darwin file NOTHING", ex.Message);
        }
    }
}
