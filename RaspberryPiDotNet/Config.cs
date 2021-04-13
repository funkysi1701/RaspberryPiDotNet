using Microsoft.Extensions.Configuration;

namespace RaspberryPiDotNet
{
    public static class Config
    {
        public static IConfiguration GetIConfiguration(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsetting.json", optional: true)
                .Build();
        }
    }
}
