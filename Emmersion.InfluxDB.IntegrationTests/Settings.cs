using System.IO;
using Microsoft.Extensions.Configuration;

namespace Emmersion.InfluxDB.IntegrationTests
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

        public IInfluxSettings InfluxSettings => new InfluxSettings
        {
            Hostname = configuration.GetValue<string>("InfluxDB:Hostname"),
            Port = configuration.GetValue<int>("InfluxDB:Port"),
            DbName = configuration.GetValue<string>("InfluxDB:DbName"),
            BatchIntervalInSeconds = 5
        };

        public T GetRequiredSetting<T>(string name)
        {
            return configuration.GetValue<T>(name);
        }
    }
}