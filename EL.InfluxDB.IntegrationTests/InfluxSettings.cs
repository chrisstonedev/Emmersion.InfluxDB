using System;

namespace EL.InfluxDB.IntegrationTests
{
    internal class InfluxSettings : IInfluxSettings
    {
        public string Hostname { get; set; } = "http://localhost";
        public int Port { get; set; } = 8086;
        public string DbName { get; set; } = "_internal";
        public int BatchIntervalInSeconds { get; set; } = 1;
        public int MaxBatchSize { get; set; } = 10000;
    }
}