using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient.Serialization;
using Serilog;

namespace DarwinClient
{
    public static class ReferenceVersion
    {
        public const int V3 = 3; 
        public const int V4 = 4;
        
        public const int Supported = V4;
    }

    public static class TimetableVersion
    {
        public const int V8 = 8;
        
        public const int Supported = V8;
    }

    /// <summary>
    /// Darwin Tiemtable and Reference Data Downloader
    /// </summary>
    public interface ITimetableDownloader
    {
        Task<TimetableReferenceFile> GetReference(DateTime date, CancellationToken token);
        Task<TimetableReferenceFile> GetLatestReference(CancellationToken token);
        Task<TimetableFile> GetTimetable(DateTime date, CancellationToken token);
        Task<TimetableFile> GetLatestTimetable(CancellationToken token);
    }

    /// <summary>
    /// Darwin timetables store
    /// </summary>
    public interface IDarwinDownloadSource
    {
        /// <summary>
        /// Read the Darwin source data
        /// </summary>
        /// <param name="searchPattern">regex to find specific source</param>
        /// <param name="token"></param>
        /// <returns>Stream of data, if multiple possible sources returns the latest</returns>
        Task<(Stream, string)> Read(string searchPattern, CancellationToken token);
    }

    /// <summary>
    /// Download file from S3
    /// </summary>
    public class TimetableDownloader(
        IDarwinDownloadSource source, 
        ILogger log,
        int timetableVersion = TimetableVersion.Supported,
        int timetableRefVersion = ReferenceVersion.Supported) : ITimetableDownloader
    {
        private string TimetableSuffix => $"_v{timetableVersion}";
        private string TimetableReferenceSuffix => $"_ref_v{timetableRefVersion}";
        
        public async Task<TimetableReferenceFile> GetReference(DateTime date, CancellationToken token)
        {
            var specificDate = $"{date:yyyyMMdd}\\d+{TimetableReferenceSuffix}";
            var (stream, name) = await source.Read(specificDate, token);
            await using (stream)
                return ExtractRefData(stream, name);
        }

        private TimetableReferenceFile ExtractRefData(Stream stream, string name)
        {
            var extractor = new ReferenceDataDeserializer(log);
            var data = extractor.Deserialize(stream, name);

            return new TimetableReferenceFile(name, timetableRefVersion, data);
        }
        
        public async Task<TimetableReferenceFile> GetLatestReference(CancellationToken token)
        {
            var (stream, name) = await source.Read($"\\d+{TimetableReferenceSuffix}", token);
            await using (stream)
                return ExtractRefData(stream, name);
        }

        public async Task<TimetableFile> GetTimetable(DateTime date, CancellationToken token)
        {
            var specificDate = $"{date:yyyyMMdd}\\d+{TimetableSuffix}";
            var (stream, name) = await source.Read(specificDate, token);
            using (stream)
                return ExtractTimetable(stream, name);
        }

        private TimetableFile ExtractTimetable(Stream stream, string name)
        {
            var extractor = new TimetableDeserializer(log);
            var data = extractor.Deserialize(stream, name);
            
            return new TimetableFile(name, timetableVersion, data);
        }

        public async Task<TimetableFile> GetLatestTimetable(CancellationToken token)
        {
            var (stream, name) = await source.Read($"\\d+{TimetableSuffix}", token);
            await using (stream)
                return ExtractTimetable(stream, name);
        }
    }
}