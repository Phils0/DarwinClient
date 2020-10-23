using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using DarwinClient;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DarwinFilesDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            using (var s3 = GetS3Client(configuration))
            {
                var outputFolder = CreateOutputFolder();
                Log.Information($"Created directory {outputFolder}", outputFolder);
                
                var source = new S3Source(s3, Log.Logger);
                var downloader = new TimetableDownloader(source, Log.Logger);

                var token = CancellationToken.None;
                var refData =  await downloader.GetReference(DateTime.Today, CancellationToken.None);
                var refString = JsonSerializer.Serialize(refData);
                var refFile = Path.Combine(outputFolder, refData.File);
                await File.WriteAllTextAsync(refFile, refString, token);
                Log.Information("Downloaded Refdata {file}", refData.File);
                
                var timetable =  await downloader.GetTimetable(DateTime.Today, token);
                var timetableString = JsonSerializer.Serialize(timetable);
                var timetableFile = Path.Combine(outputFolder, timetable.File);
                await File.WriteAllTextAsync(timetableFile, timetableString, token);
                Log.Information("Downloaded Timetable {file}", timetable.File);            
                
                Log.Information("Done");
            }
        }
        
        private static IAmazonS3 GetS3Client(IConfiguration config)
        {
            var options = config.GetAWSOptions();
            IAmazonS3 client = options.CreateServiceClient<IAmazonS3>();
            return client;
        }
        
        private static string CreateOutputFolder()
        {
            var folderName = Path.Combine(".", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(folderName);
            var timetableFolder = Path.Combine(folderName, "PPTimetable");
            Directory.CreateDirectory(timetableFolder);
            return folderName;
        }
    }
}