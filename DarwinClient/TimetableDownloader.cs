using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using DarwinClient.SchemaV16;
using Serilog;

namespace DarwinClient
{
    public static class ReferenceVersion
    {
        public const int V1 = 1;
        public const int V2 = 2;
        public const int V3 = 3;    // This is the only version supported
        public const int V99 = 99;
    }
    
    public static class  TimetableVersion
    {
        public const int V4 = 4;
        public const int V5 = 5;
        public const int V6 = 6;
        public const int V7 = 7;
        public const int V8 = 8;    // This is the only version supported
    }

    public static class DarwinS3
    {
        // Darwin AWS S3 details 
        public const string Region = "eu-west-1";
        public const string Bucket = "darwin.xmltimetable";
        public const string Prefix = "PPTimetable/";          
    }
    
    public interface ITimetableDownloader
    {
        Task<PportTimetableRef> GetReference(DateTime date, CancellationToken token);
        Task<PportTimetableRef> GetLatestReference(CancellationToken token);
        Task<PportTimetable> GetTimetable(DateTime date, CancellationToken token);
        Task<PportTimetable> GetLatestTimetable(CancellationToken token);
    }

    /// <summary>
    /// Download file from S3
    /// </summary>
    public class S3TimetableDownloader : ITimetableDownloader
    {
        private readonly IAmazonS3 _s3;
        private readonly ILogger _log;

        public S3TimetableDownloader(IAmazonS3 s3, ILogger log)
        {
            _s3 = s3;
            _log = log;
        }
        
        public async Task<PportTimetableRef> GetReference(DateTime date, CancellationToken token)
        {
            try
            {
                var specificDate = $"{date:yyyyMMdd}\\d+_ref_v{ReferenceVersion.V3}";
                var archive = await Find(specificDate, token);
                using var refData = await _s3.GetObjectAsync(archive.BucketName, archive.Key, token);
                return ExtractRefData(refData.ResponseStream, archive.Key);
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to get Darwin reference data");
                throw;
            }
        }

        private async Task<ListObjectsV2Response> ListDarwinFiles(CancellationToken token)
        {
            var objects = await _s3.ListObjectsV2Async(new ListObjectsV2Request()
            {
                BucketName = DarwinS3.Bucket,
                Prefix = DarwinS3.Prefix
            }, token);
            return objects;
        }

        private async Task<S3Object> Find(string searchPattern, CancellationToken token)
        {
            var objects = await ListDarwinFiles(token);
            var regex = new Regex(searchPattern);
            var archive = objects.S3Objects.Where(o => regex.IsMatch(o.Key)).OrderBy(s => s.Key).Last();
            return archive;
        }

        private PportTimetableRef ExtractRefData(Stream stream, string name)
        {
            var extractor = new ReferenceDataExtractor(_log);
            return extractor.Deserialize(stream, name);
        }
        
        private static readonly string AllRefFiles = $"\\d+_ref_v{ReferenceVersion.V3}";
        
        public async Task<PportTimetableRef> GetLatestReference(CancellationToken token)
        {
            try
            {
                var archive = await Find(AllRefFiles, token);
                using var refData = await _s3.GetObjectAsync(archive.BucketName, archive.Key, token);
                return ExtractRefData(refData.ResponseStream, archive.Key);
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to get Darwin reference data");
                throw;
            }
        }
        
        public async Task<PportTimetable> GetTimetable(DateTime date, CancellationToken token)
        {
            try
            {
                var specificDate = $"{date:yyyyMMdd}\\d+_v{TimetableVersion.V8}";
                var archive = await Find(specificDate, token);
                using var timetable = await _s3.GetObjectAsync(archive.BucketName, archive.Key, token);
                return ExtractTimetable(timetable.ResponseStream, archive.Key);
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to get Darwin timetable");
                throw;
            }
        }
        
        private PportTimetable ExtractTimetable(Stream stream, string name)
        {
            var extractor = new TimetableExtractor(_log);
            return extractor.Deserialize(stream, name);
        }
        
        private static readonly string AllTimetableFiles = $"\\d+_v{TimetableVersion.V8}";

        public async Task<PportTimetable> GetLatestTimetable(CancellationToken token)
        {
            try
            {
                var archive = await Find(AllTimetableFiles, token);
                using var timetable = await _s3.GetObjectAsync(archive.BucketName, archive.Key, token);
                return ExtractTimetable(timetable.ResponseStream, archive.Key);
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to get Darwin timetable");
                throw;
            }
        }
    }
}