using Microsoft.Extensions.Configuration;

namespace DarwinClient.Test
{
    public static class Configuration
    {
        public static IConfiguration GetConfiguration()
        {            
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .Build();
        }
    }
}