using System.IO;
using Microsoft.Extensions.Configuration;

namespace EL.InfluxDB.IntegrationTests
{
    public class Settings
    {
        private readonly IConfiguration configuration;

        public Settings()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Directory where the json files are located
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public T GetRequiredSetting<T>(string name)
        {
            return configuration.GetValue<T>(name);
        }
    }
}