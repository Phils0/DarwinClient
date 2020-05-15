using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using Serilog;
using DarwinClient.SchemaV16;

namespace DarwinClient
{
    public interface IExtractor<T> where T: class
    {
        /// <summary>
        /// Deserialises the xml file in the archive
        /// </summary>
        /// <returns></returns>
        T Deserialize(Stream stream, string name);
    }
    
    public abstract class ArchiveExtractor<T> : IExtractor<T> where T : class
    {
        protected readonly ILogger _logger;
        
        public ArchiveExtractor(ILogger logger)
        {
            _logger = logger;
        }

        public T Deserialize(Stream stream, string name)
        {
            var serializer = new XmlSerializer(typeof(T));
            try
            {
                using (stream)
                {
                    _logger.Information("Extracting {name}", name);
                    var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
                    var reader = new StreamReader(decompressionStream);
                    return serializer.Deserialize(reader) as T;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to deserialise {name}", name);
                throw;
            }
        }
    }
    
    public class ReferenceDataExtractor : ArchiveExtractor<PportTimetableRef>
    {
        public ReferenceDataExtractor(ILogger logger) :
            base(logger)
        {
        }
    }
    
    public class TimetableExtractor : ArchiveExtractor<PportTimetable>
    {
        public TimetableExtractor(ILogger logger) :
            base(logger)
        {
        }
    }
}