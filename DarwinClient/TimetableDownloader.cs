using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient.SchemaV16;
using DarwinClient.Serialization;
using Serilog;

namespace DarwinClient
{
    public static class ReferenceVersion
    {
        public const int V1 = 1;
        public const int V2 = 2;
        public const int V3 = 3; // This is the only version supported
        public const int V99 = 99;
    }

    public static class TimetableVersion
    {
        public const int V4 = 4;
        public const int V5 = 5;
        public const int V6 = 6;
        public const int V7 = 7;
        public const int V8 = 8; // This is the only version supported
    }

    /// <summary>
    /// Darwin Tiemtable and Reference Data Downloader
    /// </summary>
    public interface ITimetableDownloader
    {
        Task<PportTimetableRef> GetReference(DateTime date, CancellationToken token);
        Task<PportTimetableRef> GetLatestReference(CancellationToken token);
        Task<PportTimetable> GetTimetable(DateTime date, CancellationToken token);
        Task<PportTimetable> GetLatestTimetable(CancellationToken token);
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
    public class TimetableDownloader : ITimetableDownloader
    {
        private readonly IDarwinDownloadSource _source;
        private readonly ILogger _log;

        public TimetableDownloader(IDarwinDownloadSource source, ILogger log)
        {
            _source = source;
            _log = log;
        }

        public async Task<PportTimetableRef> GetReference(DateTime date, CancellationToken token)
        {
            var specificDate = $"{date:yyyyMMdd}\\d+_ref_v{ReferenceVersion.V3}";
            var (stream, name) = await _source.Read(specificDate, token);
            using (stream)
                return ExtractRefData(stream, name);
        }

        private PportTimetableRef ExtractRefData(Stream stream, string name)
        {
            var extractor = new ReferenceDataDeserializer(_log);
            var data = extractor.Deserialize(stream, name);
            data.File = name;
            return data;
        }

        private static readonly string AllRefFiles = $"\\d+_ref_v{ReferenceVersion.V3}";

        public async Task<PportTimetableRef> GetLatestReference(CancellationToken token)
        {
            var (stream, name) = await _source.Read(AllRefFiles, token);
            using (stream)
                return ExtractRefData(stream, name);
        }

        public async Task<PportTimetable> GetTimetable(DateTime date, CancellationToken token)
        {
            var specificDate = $"{date:yyyyMMdd}\\d+_v{TimetableVersion.V8}";
            var (stream, name) = await _source.Read(specificDate, token);
            using (stream)
                return ExtractTimetable(stream, name);
        }

        private PportTimetable ExtractTimetable(Stream stream, string name)
        {
            var extractor = new TimetableDeserializer(_log);
            var data = extractor.Deserialize(stream, name);
            data.File = name;
            return data;
        }

        private static readonly string AllTimetableFiles = $"\\d+_v{TimetableVersion.V8}";

        public async Task<PportTimetable> GetLatestTimetable(CancellationToken token)
        {
            var (stream, name) = await _source.Read(AllTimetableFiles, token);
            using (stream)
                return ExtractTimetable(stream, name);
        }
    }
}