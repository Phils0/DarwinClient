using Apache.NMS;

namespace DarwinClient
{
    internal static class PropertiesExtension
    {
        internal static string TryGetProperty(this IPrimitiveMap properties, string name, string defaultValue = "")
        {
            return properties.Contains(name) ? properties.GetString(name) : defaultValue;
        }
    }
}