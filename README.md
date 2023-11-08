# Darwin.Client
A .Net Standard 2.0 Library NRE Darwin Client

![Build](https://github.com/phils0/DarwinClient/actions/workflows/build.yml/badge.svg)
![Package](https://github.com/phils0/DarwinClient/actions/workflows/package.yml/badge.svg)

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