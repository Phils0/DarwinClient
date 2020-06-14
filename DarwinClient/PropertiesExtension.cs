using Apache.NMS;

namespace DarwinClient
{
    public static class PropertiesExtension
    {
        public static string TryGetProperty(this IPrimitiveMap properties, string name, string defaultValue = "")
        {
            return properties.Contains(name) ? properties.GetString(name) : defaultValue;
        }
    }
}