using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DarwinFileSink
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            var user = configuration.GetValue<string>("user");
            var password = configuration.GetValue<string>("password");
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                Log.Fatal("User or password not provided in commandline");
                Log.Information("Usage:  DarwinFileSink user=<user> password=<password>");
                return;
            }
            
            var hostUrl = configuration.GetValue<string>("DarwinUrl");
            if (string.IsNullOrEmpty(hostUrl))
            {
                Log.Fatal("DarwinUrl not set");
                Log.Information("Set DarwinUrl property, the Darwin messaging host in env variables or appSettings.json");
                return;
            }
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            
            var task = Task.Run(() => ReadQueue(hostUrl, user, password, token));
            
            Console.WriteLine("*---------------------*");
            Console.WriteLine("Enter 'quit' to exit");
            Console.WriteLine("*---------------------*");

            while((Console.ReadLine() ?? string.Empty) != "quit")
            {
            }
            source.Cancel();
            
            Log.Information("Done");
        }

        private static void ReadQueue(string darwinUrl, string user, string password, CancellationToken token)
        {
            var outputFolder = CreateOutputFolder();
            Log.Information($"Created directory {outputFolder}", outputFolder);
            var sink = CreateSink(outputFolder);
            
            using (IPushPort pushPort = new PushPort(darwinUrl, Log.Logger))
            {
                var queue = pushPort.CreateQueue();
                pushPort.Connect(user, password);
                Log.Information("Connected to pushport {pushPort}", pushPort);
                
                while(!token.IsCancellationRequested)
                {
                    while (queue.TryDequeue(out var msg))
                    {
                        var output = JsonSerializer.Serialize(msg);
                        sink.Information(output);
                    }
                    Thread.Sleep(1000);
                }
                Log.Information("Done");
            }
        }
        
        
        private static string CreateOutputFolder()
        {
            var folderName = Path.Combine(".", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            return folderName;
        }

        private static ILogger CreateSink(string folder)
        {
            var file = Path.Combine(folder, "darwin.json");
            return new LoggerConfiguration()
                .WriteTo.File(
                    // new CompactJsonFormatter(),
                    outputTemplate: "{Message:lj}",
                    path: file, 
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: 10000000,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 1000,
                    flushToDiskInterval: new TimeSpan(0, 0, 5))
                .CreateLogger();
        }
    }
}