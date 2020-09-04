# Darwin.Client
A .Net Standard 2.0 Library NRE Darwin Client

[![Build Status](https://dev.azure.com/phils0oss/DarwinClient/_apis/build/status/Phils0.DarwinClient?branchName=master)](https://dev.azure.com/phils0oss/DarwinClient/_build/latest?definitionId=1&branchName=master)
 
## How do I download the Darwin Timetable File?
 
```
// Create AWS S3 client and Serilog logger
var downloader = new S3TimetableDownloader(client, _logger);
var timetable = await downloader.GetLatestTimetable(CancellationToken.None);
```
 
See `S3TimetableDownloaderTest` for further examples of downloading the timetable and reference file.

## How do I get messages from the pushport?
 
```
//  Create Serilog logger
using IPushPort pushPort = new PushPort(pushportUrl, _logger);
var queue = pushPort.CreateQueue();
pushPort.Connect(pushportUser, pushportPassword);
_logger.Information("Connected to pushport {pushPort}", pushPort);
            
while(true)
{
    if(queue.TryDequeue(out var msg))
    {
        // Handle Darwin message
    }
}
```

If you want to create your own subscriber use the `ISubscriptionPushPort` interface on `PushPort`