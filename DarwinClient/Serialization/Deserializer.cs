using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using Serilog;

namespace DarwinClient.Serialization
{
    public abstract class Deserializer<T>(ILogger logger) : IDeserializer<T>
        where T : class
    {
        protected readonly ILogger Logger = logger;
        protected readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));

        public T? Deserialize(Stream stream, string id)
        {
            try
            {
                using (stream)
                {
                    Logger.Debug("Deserialize {id}", id);
                    using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
                    using var reader = new StreamReader(decompressionStream);
                    var obj = Serializer.Deserialize(reader);
                    return obj as T;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise {id}", id);
                throw new DarwinException($"Failed to deserialise ${id}", e);
            }
        }

        public T? Deserialize(string input, string id)
        {
            try
            {
                Logger.Debug("Deserialize {id}", id);
                using var reader = new StringReader(input);
                return Serializer.Deserialize(reader) as T;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise {id}", id);
                throw new DarwinException($"Failed to deserialise ${id}", e);
            }
        }
    }
}