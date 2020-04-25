using System.IO.Compression;
using System.Xml.Serialization;
using DarwinClient.SchemaV16;

namespace DarwinClient
{
    using System;
    using System.IO;
    using Serilog;

    public interface IArchive<T> where T: class
    {
        /// <summary>
        /// Archive File
        /// </summary>
        FileInfo File { get; }
        /// <summary>
        /// Archive File Name 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates reader to get the file from the archive
        /// </summary>
        /// <returns></returns>
        TextReader Extract();

        /// <summary>
        /// Deserialises the xml file in the archive
        /// </summary>
        /// <returns></returns>
        T Deserialize();
    }
    
    public abstract class Archive<T> : IArchive<T> where T : class
    {
        protected readonly ILogger _logger;

        public Archive(string archiveFile, ILogger logger) :
            this(new FileInfo(archiveFile), logger)
        {
        }
        
        public Archive(FileInfo archiveFile, ILogger logger)
        {
            File = archiveFile;
            _logger = logger;
        }
        
        public FileInfo File { get; }
        
        public string Name => File.Name;
        
        public TextReader Extract()
        {
            _logger.Information("Extracting {file}", File.FullName);
            var fileStream = File.OpenRead();
            var decompressionStream = new GZipStream(fileStream, CompressionMode.Decompress);
            return new StreamReader(decompressionStream);
        }

        public T Deserialize()
        {
            var serializer = new XmlSerializer(typeof(T));
            try
            {
                return serializer.Deserialize(Extract()) as T;

            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to deserialise {file}", Name);
                throw;
            }
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
    
    public class ReferenceArchive : Archive<PportTimetableRef>
    {
        public ReferenceArchive(string archiveFile, ILogger logger) :
            base(archiveFile, logger)
        {
        }
    }
    
    public class TimetableArchive : Archive<PportTimetable>
    {
        public TimetableArchive(string archiveFile, ILogger logger) :
            base(archiveFile, logger)
        {
        }
    }
}