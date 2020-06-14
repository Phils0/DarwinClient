using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using Serilog;

namespace DarwinClient.Serialization
{
    public abstract class Deserializer<T> : IDeserializer<T> where T : class
    {
        protected readonly ILogger _logger;
        protected readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));
        
        protected Deserializer(ILogger logger)
        {
            _logger = logger;
        }

        public T Deserialize(Stream stream, string id)
        {
            try
            {
                using (stream)
                {
                    _logger.Debug("Deserialize {id}", id);
                    using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
                    using var reader = new StreamReader(decompressionStream);
                    return Serializer.Deserialize(reader) as T;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to deserialise {id}", id);
                throw new DarwinException($"Failed to deserialise ${id}", e);
            }
        }

        public T Deserialize(string input, string id)
        {
            try
            {
                _logger.Debug("Deserialize {id}", id);
                using var reader = new StringReader(input);
                return Serializer.Deserialize(reader) as T;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to deserialise {id}", id);
                throw new DarwinException($"Failed to deserialise ${id}", e);
            }
        }
    }
}