using Amazon.S3;
using Microsoft.Extensions.Configuration;

namespace DarwinClient.Test.Helpers
{
    public static class Amazon
    {
        public static IAmazonS3 GetS3Client()
        {
            var config = Configuration.GetConfiguration();
            var options = config.GetAWSOptions();
            IAmazonS3 client = options.CreateServiceClient<IAmazonS3>();
            return client;
        }
    }
}