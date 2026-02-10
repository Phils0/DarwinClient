using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using NSubstitute;

namespace DarwinClient.Test.Helpers
{
    public static class DummyS3
    {
        public static DirectoryInfo Directory => new DirectoryInfo(Path.Combine(".", "Data"));
        
        public static DateTime TestDate = new DateTime(2020, 4, 29);
        public static string ReferenceData = Path.Combine(".", "Data", "20200429020643_ref_v3.xml.gz");
        public static string ReferenceDataV2 = Path.Combine(".", "Data", "20200415020643_ref_v2.xml.gz");
        public static string Timetable = Path.Combine(".", "Data", "20200429020643_v8.xml.gz");

        public static List<S3Object> DarwinFiles => new List<S3Object>()
        {
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_ref_v1.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_ref_v2.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_ref_v3.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_ref_v99.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_v4.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_v5.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_v6.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_v7.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200429020643_v8.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_ref_v1.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_ref_v2.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_ref_v3.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_ref_v99.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_v4.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_v5.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_v6.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_v7.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200430020644_v8.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_ref_v1.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_ref_v2.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_ref_v3.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_ref_v99.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_v4.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_v5.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_v6.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_v7.xml.gz"},
            new S3Object() {BucketName = "darwin.xmltimetable", Key = "PPTimetable/20200501020639_v8.xml.gz"}
        };

        private static Regex nameRegex = new Regex("^PPTimetable/(.+)$");

        private static string GetName(string key)
        {
            var match = nameRegex.Match(key);
            return match.Groups[1].Value;
        }

        public static GetObjectResponse DummyRefDataResponse(string bucket, string key)
        {
            try
            {
                var name = GetName(key);
                var file = Path.Combine(".", "Data", name);
                return GetObjectResponse(bucket, key, file);
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Warning("Dummy S3 error: {e}", e);
                return CreateDummyObject();
            }

            GetObjectResponse CreateDummyObject()
            {
                var dummy = new GetObjectResponse();
                dummy.BucketName = bucket;
                dummy.Key = key;
                dummy.ResponseStream = new MemoryStream();
                return dummy;
            }
        }

        private static GetObjectResponse GetObjectResponse(string bucket, string key, string file)
        {
            return new GetObjectResponse()
            {
                BucketName = bucket,
                Key = key,
                ResponseStream = File.OpenRead(file)
            };
        }

        public static IAmazonS3 CreateMock()
        {
            var client = CreateMockWithList();
            client.GetObjectAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(args => Task.FromResult(DummyRefDataResponse((string) args[0], (string) args[1])));

            return client;
        }

        private static IAmazonS3 CreateMockWithList()
        {
            var listResponse = new ListObjectsV2Response();
            listResponse.S3Objects = DarwinFiles;

            var client = Substitute.For<IAmazonS3>();

            client.ListObjectsV2Async(Arg.Any<ListObjectsV2Request>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(listResponse));
            return client;
        }

        public static IAmazonS3 CreateSuccessfulRefDataMock()
        {
            var client = CreateMockWithList();
            client.GetObjectAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(args => Task.FromResult(GetObjectResponse((string) args[0], (string) args[1], ReferenceData)));

            return client;
        }
        
        public static IAmazonS3 CreateSuccessfulTimetableMock()
        {
            var client = CreateMockWithList();
            client.GetObjectAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(args => Task.FromResult(GetObjectResponse((string) args[0], (string) args[1], Timetable)));

            return client;
        }
    }
}