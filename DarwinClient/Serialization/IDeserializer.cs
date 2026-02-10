using System.IO;

namespace DarwinClient.Serialization
{
    public interface IDeserializer<out T> where T: class
    {
        /// <summary>
        /// Deserialises the xml file in the archive
        /// </summary>
        /// <returns></returns>
        T? Deserialize(Stream stream, string id);
        T? Deserialize(string input, string id);       
    }
}