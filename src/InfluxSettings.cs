namespace EL.InfluxDB
{
    public class InfluxSettings
    {
        public InfluxSettings(string influxHostName, int influxPort, string influxDbName)
        {
            InfluxHostname = influxHostName;
            InfluxPort = influxPort;
            InfluxDbName = influxDbName;
        }

        public string InfluxHostname { get; }
        public int InfluxPort { get; }
        public string InfluxDbName { get; }
        public int BatchIntervalInSeconds { get; set; } = 1;
        public int MaxBatchSize { get; set; } = 10000;
    }
}