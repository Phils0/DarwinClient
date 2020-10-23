using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace DarwinClient
{
    public class FileSource : IDarwinDownloadSource
    {
        private readonly DirectoryInfo _directory;
        private readonly ILogger _log;
    
        public FileSource(DirectoryInfo directory, ILogger log)
        {
            _directory = directory;
            _log = log;
        }
            
        public Task<(Stream, string)> GetLatest(string searchPattern, CancellationToken token)
        {
            FileInfo archive = null;            
            try
            {
                archive = Find(searchPattern, token);
                if (archive == null)
                    throw new DarwinException($"Did not find Darwin file {searchPattern}");
                return Task.FromResult(( (Stream) archive.OpenRead(), archive.Name));
            }
            catch (DarwinException de)
            {
                _log.Warning(de.Message);
                throw;
            }
            catch (Exception e)
            {
                var file = archive?.Name ?? searchPattern;
                _log.Error(e, "Error loading from file system: {file}", file);
                throw new DarwinException($"Failed to download Darwin file {file}");
            }
        }     
        
        private FileInfo Find(string searchPattern, CancellationToken token)
        {
            var files = _directory.GetFiles();
            var regex = new Regex(searchPattern);
            var archive = files.Where(f => regex.IsMatch(f.Name)).OrderBy(s => s.Name).Last();
            return archive;
        }
    }
}