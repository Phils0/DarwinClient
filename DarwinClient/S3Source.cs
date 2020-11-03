using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Serilog;

namespace DarwinClient
{
    public static class DarwinS3
    {
        // Darwin AWS S3 details 
        public const string Region = "eu-west-1";
        public const string Bucket = "darwin.xmltimetable";
        public const string Prefix = "PPTimetable/";          
    }
    
    public class S3Source : IDarwinDownloadSource
    {
        private readonly IAmazonS3 _s3;
        private readonly ILogger _log;

        public S3Source(IAmazonS3 s3, ILogger log)
        {
            _s3 = s3;
            _log = log;
        }
        
        public async Task<(Stream, string)> Read(string searchPattern, CancellationToken token)
        {
            S3Object archive = null;
            try
            {
                archive = await Find(searchPattern, token);
                if (archive == null)
                    throw new DarwinException($"Did not find Darwin file {searchPattern}");
                var data = await _s3.GetObjectAsync(archive.BucketName, archive.Key, token);
                return (data.ResponseStream, archive.Key);
            }
            catch (DarwinException de)
            {
                _log.Warning(de.Message);
                throw;
            }
            catch (Exception e)
            {
                var file = archive?.Key ?? searchPattern;
                _log.Error(e, "Error downloading from S3: {file}", file);
                throw new DarwinException($"Failed to download Darwin file {file}");
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
    }
}